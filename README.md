# Space endurer

_Boli ste vybraní na špeciálnu misiu preskúmať odľahlé kúty vesmíru.
To však nie je práca na poobedie, ale na tisícky až milióny rokov,
preto cestu absolvujete v pohyblivých kapsulách, v špeciálnom kryogenickom polospánku.
Znamená to, že čas okolo vás plynie rýchlejšie a každý pohyb,
ktorý vykonáte, je v reálnom svete mnohonásobne spomalený._

_Vaša loď všetky výskumné dáta zbiera sama a dokonca sa postupne upgraduje --
vytvára si nové motory aj záložné prístroje, ktoré vás podporujú v prežití.
Loď však nie je dokonalá a vo vysokej rýchlosti sa v nej často čosi pokazí.
Práve preto sa misie účastníte vy -- vesmírni inžinieri poverení jej opravovaním.
Vašim cieľom je udržať ju funkčnú po dostatočne dlhú dobu,
aby ste objavili čo najväčší kus vesmíru a pomohli tak ľudstvu k vedeckým objavom nevídaných dôsledkov!_

## Hranie

### Princíp a cieľ hry

Hra je určená pre ľubovoľný počet hráčov,
ktorí spolupracujú na udržiavaní vesmírnej lode,
pričom úlohy, ktoré musia plniť, postupne pribúdajú,
a teda hra sa postupne sťažuje.

Cieľom hráčov je dosiahnuť čo najvyššiu prejdenú vzdialenosť,
kým im dojde kyslík a hra sa skončí.

### Objecty a sekcie

V hre sa nachádzajú štyri typy objektov,
ktoré majú tvar kocky s hranolom v polohe hore alebo dolu
(signalizujúcu ich stav $-$ hore znamená, že objekt je funkčný,
dole je že pokazený).
Objekty majú jednu zo štyroch farieb, podľa ktorej je možné odlíšiť ich typ.
Jednotlivé typy postupne rozoberieme ďalej.

Na začiatku sa jeden objekt z každého typu nachádza v centrálnej časti lode.
Postupne objekty pribúdajú do sekcií A, B alebo C,
na jedno z troch poschodí na náhodné zatiaľ neobsadené miesto.

### Vzdialenosť a motory

Červené objekty s označením MotorReactor sú zodpovedné za pohyb lode dopredu.
Loď sa pohybuje dopredu rýchlosťou, ktorá je vždy v rozsahu
$0 - n$ parsekov ($pc$) za sekudu, kde $n$ je aktuálny počet motorov.

Rýchlosť sa postupne mení podľa počtu funkčných a nefunkčných motorov $-$
každý funkčný motor ju zvyšuje o $0.1 pc/s$ za sekundu
a každý nefunkčný ju o danú hodnotu znižuje.

Prejdenú vzdialenosť vidíte vľavo hore a zobrazí sa aj po konci hry.

Motor sa v priemere pokazí raz za $30$ sekúnd.

### Kyslík a koniec hry

Na prežitie v hre musia hráči udžať rezervy kyslíka nad hodnotou $0$ %.
Aktuálne rezervy vidíte vpravo hore. Rezervy nikdy nestúpnu nad $100$ %.
V momente, kedy klesnú pod nulu, hra končí.

Kyslík vyrábajú modré objekty s označením OxyGenerator,
z ktorých každý sa v priemere pokazí raz za $30$ sekúnd.

Rezervy sa znižujú o $1$ % za sekundu za každého hráča.
Zároveň sa znižujú o $2$ % za každý pokazený OxyGenerator
a zvyšujú o $3$ % za každý funkčný OxyGenerator.

### Káble, rýchlosti a kazenie

Žltý objekt s označením WireBox sa v priemere pokazí raz za $60$ sekúnd.

Rýchlosť otvárania výťahov a dverí sa násobí hodnotou
$1$ plus počet funkčných WireBoxov a delí hodnotou
$1$ plus počet pokazených Wireboxov.

Šanca, že sa určitý objekt pokazí,
sa zvyšuje o $5$ % za každý pokazený WireBox
a znižuje o rovnakú hodnotu za každý funkčný WireBox.

### Otváranie a zatváranie dverí

Zelený objekt s označením DoorPanel sa pokazí v priemere raz za $60$ sekúnd.

Ak je pokazených viac DoorPanelov, ako je funkčných,
dvere nereagujú na otvorenie.

Dvere sa otvárajú stlačením tlačidla (alebo stúpením na tlačidlo) vedľa nich.
Zatvárajú sa rovnakým spôsobom.
Po otvorení sa automaticky začnú zatvárať po $60$ sekundách.

DoorPanel môže vzniknúť iba v časti lode,
ktorá nie je od hlavnej časti oddelená dverami.

### Výťahy

Výťahy sa ovládajú stláčaním tlačidla pred nimi (alebo stúpením naň).
Po zapnutí tlačidla sa výťah začne posúvať o jedno poschodie.

Výťah vždy ide až úplne na vrch a potom zase úplne na spodok.

### Správy

Pokiaľ sa nejaký objekt pokazí alebo vznikne nový,
loď o tom vytvorí oznámenie, ktoré spadne z lampy v centrálnej časti.

Tieto oznámenia sa dajú čítať a dajú sa zničiť.

### Pohyb po mape

Po mape sa pohybujete pomocou šípok alebo WASD,
skáčete stlačením SPACE. Otáčate sa hýbaním kurzora myši.

### Opravovanie predmetov a interakcia s nimi

S predmetmi interagujete pomocou ľavého tlačidla myši alebo tlačidla E.

Na opravenie predmetu musíte tlačidlo inteakcie držať sekundu
a mieriť pritom na predmet.

Na interakciu s tlačidlami dverí a výťahov stačí tlačidlo interakcie stlačiť.

Na čítanie oznámenia naň treba buď kliknúť ľavým tlačidlom myši
alebo naň zamieriť a držať E.
Ukáže sa tým text oznámenia.
Prestať čítať oznámenie sa dá buď stlačením tlačidla E
(ak bolo čítané stlačením myši)
alebo pustením tlačidla E (ak bolo čítané jeho držaním)
alebo stlačením tlačidla Q.
Stlačenie Q zároveň oznámenie zničí.

## Pripájanie sa

Na začatie hry musí jeden hráč v menu zvoliť Create new game
(pritom musí vyplniť iba svoje meno a nie adresu serveru).
Tým u seba vytvorí hru a jeho zariadenie sa zároveň použije ako server.
Po odpojení serveru sa v hre nedá pokračovať.

Ostatní hráči musia vyplniť meno a adresu, ktorú sa dozvedia od prvého hráča
a zvoliť Connect.

Z hry sa hráči môžu odpojiť stlačením ľavého Shiftu + Escape.
Stlačením samotného Escape alebo P sa pohyb hráča zapauzuje
a jeho kurzor uvoľní.
Zároveň sa objaví možnosť odpojiť sa z hry.
Pre návrat z pauzového menu stačí stlačiť Escape alebo P.
