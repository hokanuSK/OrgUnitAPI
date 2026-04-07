#!/usr/bin/env bash
set -euo pipefail

BASE_URL="${BASE_URL:-http://localhost:5132/api}"
COMPANY_CODE="KROS$(date +%s)"
EMAIL="jan.$(date +%s)@kros.sk"

request() {
  local method="$1"
  local url="$2"
  local body="${3:-}"
  local tmp
  local code

  tmp=$(mktemp)
  if [[ -n "$body" ]]; then
    code=$(curl -sS -o "$tmp" -w '%{http_code}' -X "$method" "$url" -H 'Content-Type: application/json' -d "$body")
  else
    code=$(curl -sS -o "$tmp" -w '%{http_code}' -X "$method" "$url")
  fi

  echo "$code|$tmp"
}

assert_status() {
  local actual="$1"
  local expected="$2"
  local label="$3"

  if [[ "$actual" != "$expected" ]]; then
    echo "FAIL: $label expected=$expected actual=$actual"
    exit 1
  fi

  echo "PASS: $label status=$actual"
}

echo "Using BASE_URL=$BASE_URL"

r=$(request POST "$BASE_URL/companies" "{\"code\":\"$COMPANY_CODE\",\"name\":\"KROS a.s.\"}")
code=${r%%|*}; file=${r#*|}
assert_status "$code" "201" "Create company"
companyId=$(jq -r '.companyId' "$file")

echo "companyId=$companyId"

r=$(request POST "$BASE_URL/companies/$companyId/employees" "{\"title\":\"Ing.\",\"firstName\":\"Jan\",\"lastName\":\"Novak\",\"phone\":\"+421900111222\",\"email\":\"$EMAIL\"}")
code=${r%%|*}; file=${r#*|}
assert_status "$code" "201" "Create employee"
employeeId=$(jq -r '.employeeId' "$file")

echo "employeeId=$employeeId"

r=$(request PUT "$BASE_URL/companies/$companyId" "{\"code\":\"$COMPANY_CODE\",\"name\":\"KROS a.s.\",\"directorEmployeeId\":$employeeId}")
code=${r%%|*}
assert_status "$code" "200" "Set company director"

r=$(request POST "$BASE_URL/companies/$companyId/org-units" '{"unitType":"Project","code":"PRJ-INVALID","name":"Invalid Project"}')
code=${r%%|*}
assert_status "$code" "422" "Invalid project without parent"

r=$(request POST "$BASE_URL/companies/$companyId/org-units" "{\"unitType\":\"Division\",\"code\":\"DIV-IT\",\"name\":\"IT Division\",\"leaderEmployeeId\":$employeeId}")
code=${r%%|*}; file=${r#*|}
assert_status "$code" "201" "Create division"
divisionId=$(jq -r '.orgUnitId' "$file")

r=$(request POST "$BASE_URL/companies/$companyId/org-units" "{\"unitType\":\"Project\",\"parentOrgUnitId\":$divisionId,\"code\":\"PRJ-ERP\",\"name\":\"ERP Platform\",\"leaderEmployeeId\":$employeeId}")
code=${r%%|*}; file=${r#*|}
assert_status "$code" "201" "Create project"
projectId=$(jq -r '.orgUnitId' "$file")

r=$(request POST "$BASE_URL/companies/$companyId/org-units" "{\"unitType\":\"Department\",\"parentOrgUnitId\":$projectId,\"code\":\"DEP-BE\",\"name\":\"Backend Department\",\"leaderEmployeeId\":$employeeId}")
code=${r%%|*}
assert_status "$code" "201" "Create department"

r=$(request GET "$BASE_URL/companies/$companyId/org-tree")
code=${r%%|*}
assert_status "$code" "200" "Get org tree"

r=$(request POST "$BASE_URL/companies/$companyId/employees" '{"firstName":"No","lastName":"Email"}')
code=${r%%|*}
assert_status "$code" "400" "Validation missing email"

r=$(request POST "$BASE_URL/companies/$companyId/employees" "{\"title\":\"Ing.\",\"firstName\":\"Jan2\",\"lastName\":\"Novak2\",\"phone\":\"+421900000000\",\"email\":\"$EMAIL\"}")
code=${r%%|*}
assert_status "$code" "409" "Duplicate email"

echo "All API checks passed."
