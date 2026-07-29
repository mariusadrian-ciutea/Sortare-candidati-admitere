# Sortare candidati admitere

Proiectul contine o aplicatie Windows pentru gestionarea si repartizarea candidatilor la admitere, plus un site web prin care candidatii isi pot trimite formularul de inscriere.

## Structura

- `Proiect_admitere facultate/` - aplicatie Windows Forms pe .NET Framework 4.7.2.
- `Site_inscriere_admitere/` - site web pentru inscriere, construit cu Vinext/React si API-uri pentru salvarea formularului.

## Functionalitati

- preluarea inscrierilor trimise prin site;
- listarea, cautarea si actualizarea candidatilor;
- repartizarea candidatilor in functie de medie si optiuni;
- persistenta datelor in baza LocalDB a aplicatiei;
- formular online pentru date personale, medii si optiuni de specializare.

## Rulare aplicatie Windows

Deschide solutia:

`Proiect_admitere facultate/Proiect_admitere facultate.sln`

Compileaza in `Release`, apoi porneste executabilul din:

`Proiect_admitere facultate/bin/Release/Proiect_admitere facultate.exe`

## Rulare site

Din folderul `Site_inscriere_admitere/`:

```bash
npm install
npm run dev
```

Pentru build:

```bash
npm run build
```

## Configurare

Aplicatia Windows foloseste `WebApiBaseUrl` si `WebImportToken` in `App.config` pentru importul inscrierilor.

Site-ul foloseste variabila `ADMISSION_IMPORT_TOKEN`, definita local in `.env` dupa modelul din `.env.example`.

Baza locala cu date reale nu este inclusa in repository. Pentru o baza noua, creeaza o baza LocalDB si ruleaza scriptul:

`Proiect_admitere facultate/SQLQuery2.sql`

Nu se recomanda publicarea token-urilor reale in repository.
