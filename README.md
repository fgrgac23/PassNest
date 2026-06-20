# PassNest
**Sveučilište u Zagrebu Fakultet organizacije i informatike, Varaždin**

Studij: Informacijski i poslovni sustavi / Razvoj programskih sustava

Mentor: Vjeran Strahonja, prof. emer.

Završni rad: **Dizajn temeljen na komponentama**

## Opis domene
Upravljanje korisničkim računima zahtjeva pamćenje velikog broja kombinacija korisničkih imena i lozinki za različite servise i aplikaicje. Najčešći problem korisnika je zaboravljena lozinka i korištenje iste ili slabe lozinke na više mjesta. Takvi problemi su jedan od najčešćih uzroka sigurnosnih incidenata i neovlaštenog pristupa računima.

**PassNest** je desktop aplikacija namijenjena pojedinačnom korisniku za sigurno i organizirano čuvanje pristupnih podataka (korisničko ime/e-mail i lozinka) za neograničen broj korisničkih računa na različitim servisima. Svaki spremljeni račun, s oznakom servisa, prikazuje se korisniku u obliku zasebne kartice na početnom zaslonu aplikacije.

Apliakcija je osmišljena kao primjer dizajna temeljenog na komponentama (component-based design). Svaka funkcionalna cjelina (prikaz kartica, generator lozinki, enkripcijski modul, pritup bazi podataka, modul za autentifikaciju, modul za automatsko popunjavanje obrazaca) razvija se kao samostalna, ponovno upotrebljiva komponenta s jasno definiranim sučeljem prema ostatku sustava. Ovakav pristup omogućuje lakše testiranje, održavanje i proširivost aplikaicje.

Podaci se pohranjuju lokalno, u enkriptiranom obliku, čime se smanjuje mogućnost za potencijalne sigurnosne napade, dok korisnik ostaje jedini vlasnik svojih podataka.

## Specifikacija zahtjeva
| Oznaka | Naziv | Opis |
| ------ | ----- | ---- |
| FZ-01 | Autentikacija | Omogućuje novim korisnicima kreiranje računa. Sustav validira unesene podatke i sprema ih u bazu. Zatim se korisnici prijavljuju pomoću registriranih podataka. |
| FZ-02 | Dodavanje korisničkog računa | Korisnik može dodati novi zapis (račun) unosom naziva servisa, korisničkog imena/e-maila i lozinke. |
| FZ-03 | Prikaz računa u obliku kartica | Svi spremljeni računi prikazuju se na početnom zaslonu u obliku kartice s nazivom servisa i osnovnim podacima. |
| FZ-04 | Uređivanje postojećeg računa | Korisnik može izmijeniti podatke (naziv, korisničko ime, lozinku) već spremljenog računa. |
| FZ-05 | Brisanje korisničkog računa | Korisnik može trajno ukloniti odabrani zapis iz baze podataka. |
| FZ-06 | Generiranje nasumične lozinke | Aplikacija generira nasumičnu lozinku prema odabranim paramterima (dužina, velika/mala slova, brojevi, posebni znakovi). |
| FZ-07 | Kategorizacija računa | Korisnik može dodijeliti kategoriju/oznaku svakom računu radi lakšeg filtriranja i pregleda. |
| FZ-08 | Pretraživanje i filtriranje računa | Korisnik može pretraživati spremljene račune po nazivu servisa ili filtriranje prikaza po kategoriji. |
| FZ-09 | Kopiranje podataka u međuspremnik | Korisnik može jednim klikom kopirati korisničko ime ili lozinku u međuspremnik radi jednostavnog korištenja u drugim aplikaicjama. |
| FZ-10 | Automatsko popunjavanje obrazaca | Na zahtjev korisnika, aplikacija putem simulacije unosa tipkovnice popunjava prijavna polja (korisničko ime i lozinka) u trenutno aktivnom prozoru drugog programa. |
| FZ-11 | Inicijalno postavljanje master lozinke | Prilikom registracije korisnik definira master lozinku iz koje se izvodi ključ za enkripciju svih budućih podataka. |
| FZ-12 | Prijava putem master lozinke | Pri svakom pokretanju aplikacije, detaljan pristup spremljenim podacima omogućen je isključivo unosom prethodno postavljene master lozinke. |
| FZ-13 | Provjera sigurnosti spremljenih lozinka | Korisnik može pokrenuti analizu spremljenih lozinki i upozoriti korisnika na slabe lozinke. |
| FZ-14 | Pristup aplikacije putem globalne tipkovničke kombinacije | Korisnik može u svakom trenutnu prikazati glavni prozor apliakcije pomoću unaprijed definirane kombinacije tipki, neovisno o tome koja je aplikacija trenutno aktivna. |
| FZ-15 | Opcionalna dvofaktorska prijava putem e-maila | Korisnik može, po želji, omogućiti dodatni faktor autorizacije putem e-maila prilikom prijave u aplikaciju gdje se na e-mail šalje jednokratni kod koji je potrebno unijeti za potpunu prijavu. |
