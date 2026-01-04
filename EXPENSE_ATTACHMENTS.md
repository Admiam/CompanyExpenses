# Expense Attachments - Dokumentace

## Přehled

Systém pro nahrávání, správu a stahování příloh (účtenek, dokladů) k výdajům.

## Backend API

### ExpenseAttachmentsController

**Základní URL:** `/api/expenses/{expenseId}/expenseattachments`

#### Endpointy

1. **GET** - Získat všechny přílohy výdaje

   ```
   GET /api/expenses/{expenseId}/expenseattachments
   ```

2. **POST** - Nahrát přílohu

   ```
   POST /api/expenses/{expenseId}/expenseattachments
   Content-Type: multipart/form-data

   Body:
   - file: File (required)
   - userId: string (optional)
   ```

   **Omezení:**

   - Maximální velikost: 10 MB
   - Povolené typy: JPEG, PNG, GIF, PDF

3. **GET {id}** - Stáhnout přílohu

   ```
   GET /api/expenses/{expenseId}/expenseattachments/{id}
   ```

   Vrací soubor ke stažení.

4. **DELETE {id}** - Smazat přílohu
   ```
   DELETE /api/expenses/{expenseId}/expenseattachments/{id}
   ```

### Konfigurace

V `appsettings.json`:

```json
{
  "FileStorage": {
    "UploadPath": "uploads/receipts"
  }
}
```

Soubory se ukládají do složky specifikované v konfiguraci. Složka se vytvoří automaticky, pokud neexistuje.

## Frontend

### Komponenta: ExpenseFormModal

Rozšířený formulář pro výdaje s možností nahrávání účtenky.

**Features:**

- Drag & drop interface pro nahrávání souborů
- Preview obrázků před odesláním
- Zobrazení existujících příloh
- Validace typu a velikosti souboru
- Formátování velikosti souboru

### API Utility

`src/utils/expenseAttachments.ts`

```typescript
// Získat přílohy
const attachments = await getExpenseAttachments(expenseId);

// Nahrát přílohu
const attachment = await uploadExpenseAttachment(expenseId, file, userId);

// URL pro stažení
const downloadUrl = getAttachmentDownloadUrl(expenseId, attachmentId);

// Smazat přílohu
await deleteExpenseAttachment(expenseId, attachmentId);

// Formátovat velikost souboru
const formattedSize = formatFileSize(1024000); // "1000.0 KB"
```

## Použití

### 1. Vytvoření výdaje s přílohou

```typescript
const handleSaveExpense = async (data: any) => {
  // Nejprve vytvoř výdaj
  const expense = await createExpense({
    description: data.description,
    amount: data.amount,
    expenseDate: data.expenseDate,
    categoryId: data.categoryId,
    workplaceId: data.workplaceId,
    currency: data.currency,
  });

  // Pokud je přiložen soubor, nahraj ho
  if (data.file && expense.id) {
    try {
      await uploadExpenseAttachment(expense.id, data.file, currentUserId);
      console.log("Attachment uploaded successfully");
    } catch (error) {
      console.error("Failed to upload attachment:", error);
      // Můžeš zobrazit upozornění uživateli
    }
  }
};
```

### 2. Zobrazení příloh existujícího výdaje

```typescript
useEffect(() => {
  if (expense?.id) {
    loadAttachments(expense.id);
  }
}, [expense]);

const loadAttachments = async (expenseId: string) => {
  try {
    const attachments = await getExpenseAttachments(expenseId);
    setExistingAttachments(attachments);
  } catch (error) {
    console.error("Failed to load attachments:", error);
  }
};
```

### 3. Stažení přílohy

```tsx
<a href={getAttachmentDownloadUrl(expenseId, attachment.id)} download={attachment.originalFileName} target="_blank" rel="noopener noreferrer">
  Stáhnout
</a>
```

## Databázová struktura

### Tabulka: ExpenseAttachments

| Sloupec          | Typ      | Popis                                      |
| ---------------- | -------- | ------------------------------------------ |
| Id               | Guid     | Primární klíč                              |
| ExpenseId        | Guid     | Foreign key na Expenses                    |
| OriginalFileName | string   | Původní název souboru                      |
| StoredFileName   | string   | Název souboru na disku (GUID)              |
| DataType         | string   | MIME type (image/jpeg, application/pdf...) |
| FileSize         | long     | Velikost v bytech                          |
| UploadedByUserId | string   | Foreign key na AspNetUsers                 |
| UploadedAt       | DateTime | Datum a čas nahrání                        |

## Bezpečnost

- ✅ Validace typu souboru (pouze obrázky a PDF)
- ✅ Limit velikosti souboru (10 MB)
- ✅ Unikátní názvy souborů (GUID) pro prevenci kolizí
- ✅ Oddělené úložiště od zdrojového kódu
- ⚠️ TODO: Autorizace - zkontrolovat, že uživatel má přístup k výdaji
- ⚠️ TODO: Antivirová kontrola souborů
- ⚠️ TODO: Rate limiting pro upload

## TODO

- [ ] Implementovat autorizaci v controlleru
- [ ] Přidat podporu pro více souborů najednou
- [ ] Implementovat miniaturky pro obrázky
- [ ] Přidat možnost přetažení souborů (drag & drop)
- [ ] Implementovat progress bar pro nahrávání
- [ ] Přidat cloud storage support (Azure Blob, AWS S3)
