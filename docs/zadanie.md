# Zadanie REST API – Organizačná štruktúra firmy
## Požiadavky na projekt

- Zdrojový kód pošlite odkazom na GitHub. Musí byť niekde spísané, ako sa projekt jednoducho rozbehne/spustí (nezabudnite na vytvorenie databázy – stačí, aby bol k dispozícii SQL skript).
- Vytvorte REST API, ktoré umožní spravovať organizačné štruktúry firiem a evidovať ich zamestnancov.
- Do projektu zakomponujte Scalar, aby som si vedel pozrieť a vyskúšať endpointy. Na testovanie endpointov použite nástroj TeaPie.
- Použite .NET, jazyk C#, a pre úložisko dát použite Microsoft SQL Server (verzia Express je zadarmo).

## Požiadavky na API
 
- Umožniť spravovať (vytvárať/upravovať/mazať) max. 4-úrovňovú hierarchickú organizačnú štruktúru firiem: firmy → divízie → projekty → oddelenia.
- Každý z uzlov organizačnej štruktúry bude pomenovaný názvom a kódom a bude mať svojho vedúceho (firma – riaditeľ, divízia – vedúci divízie, projekt – vedúci projektu, oddelenie – vedúci oddelenia). Vedúci uzla je jeden zo zamestnancov firmy.
- Umožniť pridávať, meniť a vymazávať zamestnancov firiem.
- Pre zamestnanca sa bude evidovať minimálne titul, meno a priezvisko, telefón a e-mail.
- API má mať základné validácie, t. j. správne vrátiť chybu, ak nie sú zadané povinné hodnoty alebo sú inak nesprávne.
- Organizačná štruktúra aj zoznam zamestnancov budú uložené v databáze.
 
