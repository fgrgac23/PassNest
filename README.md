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
| FZ-01  | Autentikacija |  |
| FZ-02  | Dodavanje korisničkog računa | Korisnik može dodati novi zapis (račun) unosom naziva servisa, korisničkog imena/e-maila i lozinke. |
| FZ-03  | Prikaz računa u obliku kartica | Svi spremljeni računi prikazuju se na početnom zaslonu u obliku kartice s nazivom servisa i osnovnim podacima. |
| FZ-04  | Uređivanje postojećeg računa | Korisnik može izmijeniti podatke (naziv, korisničko ime, lozinku) već spremljenog računa. |
| FZ-05  | Brisanje korisničkog računa | Korisnik može trajno ukloniti odabrani zapis iz baze podataka. |
| FZ-06  | Generiranje nasumične lozinke | Aplikacija generira nasumičnu lozinku prema odabranim paramterima (dužina, velika/mala slova, brojevi, posebni znakovi). |
| FZ-07  | Kategorizacija računa | Korisnik može dodijeliti kategoriju/oznaku svakom računu radi lakšeg filtriranja i pregleda. |
