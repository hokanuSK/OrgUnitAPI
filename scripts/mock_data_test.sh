#!/usr/bin/env bash
set -euo pipefail

BASE_URL="${BASE_URL:-http://localhost:5132/api}"
TS=$(date +%s)

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
    echo "FAIL: $label expected=$expected actual=$actual" >&2
    exit 1
  fi

  echo "PASS: $label status=$actual" >&2
}

extract_json_field() {
  local file="$1"
  local expr="$2"
  jq -r "$expr" "$file"
}

create_company() {
  local code="$1"
  local name="$2"
  local r code_http file

  r=$(request POST "$BASE_URL/companies" "{\"code\":\"$code\",\"name\":\"$name\"}")
  code_http=${r%%|*}; file=${r#*|}
  assert_status "$code_http" "201" "Create company $code"
  extract_json_field "$file" '.companyId'
}

create_employee() {
  local company_id="$1"
  local title="$2"
  local first_name="$3"
  local last_name="$4"
  local email="$5"
  local phone="$6"
  local r code_http file

  r=$(request POST "$BASE_URL/companies/$company_id/employees" "{\"title\":\"$title\",\"firstName\":\"$first_name\",\"lastName\":\"$last_name\",\"phone\":\"$phone\",\"email\":\"$email\"}")
  code_http=${r%%|*}; file=${r#*|}
  assert_status "$code_http" "201" "Create employee $first_name $last_name"
  extract_json_field "$file" '.employeeId'
}

create_org_unit() {
  local company_id="$1"
  local unit_type="$2"
  local code="$3"
  local name="$4"
  local leader_id="$5"
  local parent_id="${6:-}"
  local payload
  local r code_http file

  if [[ -n "$parent_id" ]]; then
    payload="{\"unitType\":\"$unit_type\",\"parentOrgUnitId\":$parent_id,\"code\":\"$code\",\"name\":\"$name\",\"leaderEmployeeId\":$leader_id}"
  else
    payload="{\"unitType\":\"$unit_type\",\"code\":\"$code\",\"name\":\"$name\",\"leaderEmployeeId\":$leader_id}"
  fi

  r=$(request POST "$BASE_URL/companies/$company_id/org-units" "$payload")
  code_http=${r%%|*}; file=${r#*|}
  assert_status "$code_http" "201" "Create $unit_type $code"
  extract_json_field "$file" '.orgUnitId'
}

set_director() {
  local company_id="$1"
  local code="$2"
  local name="$3"
  local director_id="$4"
  local r code_http

  r=$(request PUT "$BASE_URL/companies/$company_id" "{\"code\":\"$code\",\"name\":\"$name\",\"directorEmployeeId\":$director_id}")
  code_http=${r%%|*}
  assert_status "$code_http" "200" "Set company $company_id director"
}

check_array_len() {
  local url="$1"
  local expected="$2"
  local label="$3"
  local r code_http file actual

  r=$(request GET "$url")
  code_http=${r%%|*}; file=${r#*|}
  assert_status "$code_http" "200" "$label"
  actual=$(extract_json_field "$file" 'length')

  if [[ "$actual" != "$expected" ]]; then
    echo "FAIL: $label expected_length=$expected actual_length=$actual" >&2
    exit 1
  fi

  echo "PASS: $label length=$actual"
}

check_tree_shape() {
  local company_id="$1"
  local expected_divisions="$2"
  local expected_projects="$3"
  local expected_departments="$4"
  local r code_http file divisions projects departments

  r=$(request GET "$BASE_URL/companies/$company_id/org-tree")
  code_http=${r%%|*}; file=${r#*|}
  assert_status "$code_http" "200" "Get org tree for company $company_id"

  divisions=$(extract_json_field "$file" '.divisions | length')
  projects=$(extract_json_field "$file" '[.divisions[].children[]] | length')
  departments=$(extract_json_field "$file" '[.divisions[].children[].children[]] | length')

  if [[ "$divisions" != "$expected_divisions" || "$projects" != "$expected_projects" || "$departments" != "$expected_departments" ]]; then
    echo "FAIL: org tree shape expected=($expected_divisions,$expected_projects,$expected_departments) actual=($divisions,$projects,$departments)" >&2
    exit 1
  fi

  echo "PASS: org tree shape divisions=$divisions projects=$projects departments=$departments"
}

echo "Using BASE_URL=$BASE_URL"

a_code="ORG-A-$TS"
b_code="ORG-B-$TS"
a_name="OrgUnitAPI Alpha"
b_name="OrgUnitAPI Beta"

company_a=$(create_company "$a_code" "$a_name")
company_b=$(create_company "$b_code" "$b_name")

a_dir=$(create_employee "$company_a" "Ing." "Alice" "Director" "alice.$TS@orgunitapi.local" "+421900100001")
a_div_lead=$(create_employee "$company_a" "Mgr." "Bob" "Division" "bob.$TS@orgunitapi.local" "+421900100002")
a_proj_lead=$(create_employee "$company_a" "Bc." "Carol" "Project" "carol.$TS@orgunitapi.local" "+421900100003")
a_dep_lead=$(create_employee "$company_a" "Ing." "David" "Department" "david.$TS@orgunitapi.local" "+421900100004")

b_dir=$(create_employee "$company_b" "Ing." "Eva" "Director" "eva.$TS@orgunitapi.local" "+421900200001")
b_lead=$(create_employee "$company_b" "Mgr." "Frank" "Lead" "frank.$TS@orgunitapi.local" "+421900200002")

set_director "$company_a" "$a_code" "$a_name" "$a_dir"
set_director "$company_b" "$b_code" "$b_name" "$b_dir"

a_div_1=$(create_org_unit "$company_a" "Division" "A-DIV-ENG" "Engineering" "$a_div_lead")
a_prj_1=$(create_org_unit "$company_a" "Project" "A-PRJ-PLAT" "Platform" "$a_proj_lead" "$a_div_1")
_=$(create_org_unit "$company_a" "Department" "A-DEP-BE" "Backend" "$a_dep_lead" "$a_prj_1")

a_div_2=$(create_org_unit "$company_a" "Division" "A-DIV-OPS" "Operations" "$a_div_lead")
a_prj_2=$(create_org_unit "$company_a" "Project" "A-PRJ-INFRA" "Infrastructure" "$a_proj_lead" "$a_div_2")
_=$(create_org_unit "$company_a" "Department" "A-DEP-SRE" "SRE" "$a_dep_lead" "$a_prj_2")

b_div_1=$(create_org_unit "$company_b" "Division" "B-DIV-SALES" "Sales" "$b_lead")
b_prj_1=$(create_org_unit "$company_b" "Project" "B-PRJ-CRM" "CRM" "$b_lead" "$b_div_1")
b_dep_1=$(create_org_unit "$company_b" "Department" "B-DEP-SUP" "Support" "$b_lead" "$b_prj_1")

check_array_len "$BASE_URL/companies/$company_a/employees" "4" "List employees company A"
check_array_len "$BASE_URL/companies/$company_a/org-units" "6" "List org units company A"
check_tree_shape "$company_a" "2" "2" "2"
check_array_len "$BASE_URL/companies/$company_b/employees" "2" "List employees company B"
check_array_len "$BASE_URL/companies/$company_b/org-units" "3" "List org units company B"
check_tree_shape "$company_b" "1" "1" "1"

# Duplicate company code should fail.
r=$(request POST "$BASE_URL/companies" "{\"code\":\"$a_code\",\"name\":\"Duplicate Company\"}")
code_http=${r%%|*}
assert_status "$code_http" "409" "Duplicate company code validation"

# Duplicate org unit code in same company should fail.
r=$(request POST "$BASE_URL/companies/$company_a/org-units" "{\"unitType\":\"Division\",\"code\":\"A-DIV-ENG\",\"name\":\"Duplicate Division\",\"leaderEmployeeId\":$a_div_lead}")
code_http=${r%%|*}
assert_status "$code_http" "409" "Duplicate org unit code validation"

# Invalid hierarchy update should fail (Project -> Department under Division parent).
r=$(request PUT "$BASE_URL/org-units/$a_prj_1" "{\"unitType\":\"Department\",\"parentOrgUnitId\":$a_div_1,\"code\":\"A-PRJ-PLAT\",\"name\":\"Platform Invalid\",\"leaderEmployeeId\":$a_proj_lead}")
code_http=${r%%|*}
assert_status "$code_http" "422" "Invalid hierarchy update validation"

# Cross-company leader should fail.
r=$(request POST "$BASE_URL/companies/$company_b/org-units" "{\"unitType\":\"Division\",\"code\":\"B-DIV-INVALID\",\"name\":\"Invalid Cross Leader\",\"leaderEmployeeId\":$a_dir}")
code_http=${r%%|*}
assert_status "$code_http" "422" "Cross-company leader validation"

# Cross-company director should fail.
r=$(request PUT "$BASE_URL/companies/$company_b" "{\"code\":\"$b_code\",\"name\":\"$b_name\",\"directorEmployeeId\":$a_dir}")
code_http=${r%%|*}
assert_status "$code_http" "422" "Cross-company director validation"

# Deleting parent node with children should fail.
r=$(request DELETE "$BASE_URL/org-units/$a_div_1")
code_http=${r%%|*}
assert_status "$code_http" "409" "Delete parent org unit with children"

# Delete leaf-to-root should succeed.
r=$(request DELETE "$BASE_URL/org-units/$b_dep_1")
code_http=${r%%|*}
assert_status "$code_http" "204" "Delete leaf department"

r=$(request DELETE "$BASE_URL/org-units/$b_prj_1")
code_http=${r%%|*}
assert_status "$code_http" "204" "Delete project after child removal"

r=$(request DELETE "$BASE_URL/org-units/$b_div_1")
code_http=${r%%|*}
assert_status "$code_http" "204" "Delete division after child removal"

check_array_len "$BASE_URL/companies/$company_b/org-units" "0" "List org units company B after cleanup"
check_tree_shape "$company_b" "0" "0" "0"

# Director delete and company delete should fail while employees remain.
r=$(request DELETE "$BASE_URL/employees/$b_dir")
code_http=${r%%|*}
assert_status "$code_http" "409" "Delete director employee should fail"

r=$(request DELETE "$BASE_URL/companies/$company_b")
code_http=${r%%|*}
assert_status "$code_http" "409" "Delete company with employees should fail"

echo "Mock data integration test passed."
echo "Created companies: A=$company_a B=$company_b"
