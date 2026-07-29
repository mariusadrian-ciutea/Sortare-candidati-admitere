# Site inscriere admitere

Site web pentru inscrierea candidatilor la admitere. Formularul colecteaza datele candidatului, mediile si optiunile de specializare, apoi salveaza inscrierea pentru preluarea in aplicatia Windows de repartizare.

## Comenzi

```bash
npm install
npm run dev
npm run build
```

## Configurare

Creeaza local un fisier `.env` dupa modelul din `.env.example` si seteaza `ADMISSION_IMPORT_TOKEN`.

Token-ul trebuie sa fie acelasi cu `WebImportToken` din aplicatia Windows.
