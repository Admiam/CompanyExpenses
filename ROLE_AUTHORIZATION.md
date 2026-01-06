# Omezení přístupu pro uživatele s rolí "User"

Tato dokumentace popisuje implementaci rolové autorizace, která omezuje přístup běžných uživatelů (role "User") k určitým stránkám aplikace.

## Přehled změn

Implementováno omezení přístupu pro uživatele s rolí "User" k následujícím stránkám:

- **Pracoviště** (`/workplaces`)
- **Uživatelé** (`/users`)
- **Kategorie** (`/categories`)

Tyto stránky jsou nyní dostupné pouze pro uživatele s rolemi **Admin** nebo **Manager**.

## Implementované soubory

### 1. `/src/utils/roles.ts`

Nový soubor s utilitami pro kontrolu rolí:

- `hasRole()` - kontrola konkrétní role
- `hasAnyRole()` - kontrola více rolí
- `isAdmin()` - kontrola admin role
- `isManagerOrAdmin()` - kontrola manager nebo admin role
- `canAccessWorkplaces()` - kontrola přístupu k pracovištím
- `canAccessUsers()` - kontrola přístupu k uživatelům
- `canAccessCategories()` - kontrola přístupu ke kategoriím

### 2. `/src/components/RoleProtectedRoute.tsx`

Nová komponenta pro ochranu routes na základě rolí:

- Kontroluje, zda je uživatel přihlášen
- Ověřuje, zda má uživatel požadovanou roli
- Přesměruje na dashboard, pokud uživatel nemá oprávnění
- Zobrazuje loading state během kontroly

### 3. `/src/main.tsx`

Upraveno routing pro omezené stránky:

```tsx
<Route
  path="/workplaces"
  element={
    <ProtectedRoute>
      <RoleProtectedRoute requiredRoles={["Admin", "Manager"]}>
        <WorkplacesPage />
      </RoleProtectedRoute>
    </ProtectedRoute>
  }
/>
```

### 4. `/src/components/app-sidebar.tsx`

Upraveno filtrování navigačních položek:

- Sidebar nyní zobrazuje pouze položky, ke kterým má uživatel přístup
- Používá funkce z `roles.ts` pro kontrolu oprávnění
- Běžní uživatelé neuvidí odkazy na Pracoviště, Uživatele a Kategorie

## Jak to funguje

### Systém rolí

Aplikace používá tři role definované v Microsoft Identity:

- **Admin** - plný přístup ke všem stránkám
- **Manager** - přístup ke všem stránkám kromě některých admin funkcí
- **User** - omezený přístup (pouze Dashboard a Výdaje)

### Ochrana routes

Stránky jsou chráněny dvěma vrstvami:

1. **ProtectedRoute** - vyžaduje přihlášení
2. **RoleProtectedRoute** - vyžaduje konkrétní role

### Skrytí navigace

Navigační položky v sidebaru jsou dynamicky filtrovány na základě role uživatele:

- Uživatel vidí pouze odkazy, ke kterým má přístup
- Pokud se pokusí zadat URL přímo, je přesměrován na dashboard

## Testování

### Testovací scénáře:

1. **Běžný uživatel (User)**:

   - ✅ Vidí: Dashboard, Výdaje
   - ❌ Nevidí: Pracoviště, Uživatelé, Kategorie
   - ❌ Pokus o přímý přístup URL: přesměrování na dashboard

2. **Manager**:

   - ✅ Vidí všechny položky menu
   - ✅ Má přístup ke všem stránkám

3. **Admin**:
   - ✅ Vidí všechny položky menu
   - ✅ Má přístup ke všem stránkám

## Poznámky k bezpečnosti

⚠️ **Důležité**: Toto je pouze frontend ochrana. Pro plnou bezpečnost je nutné:

1. Implementovat autorizaci na API endpointech
2. Kontrolovat role na backendu před každou operací
3. Validovat oprávnění v databázových dotazech

Frontend ochrana slouží pouze pro UX - skrývá nedostupné funkce a zabraňuje náhodnému přístupu, ale není to bezpečnostní opatření.

## Rozšíření

Pro přidání dalších omezení:

1. Přidejte novou funkci do `/src/utils/roles.ts`
2. Použijte `RoleProtectedRoute` s požadovanými rolemi
3. Přidejte filtr do `app-sidebar.tsx` pro skrytí menu položky

Příklad:

```tsx
// roles.ts
export function canAccessReports(userRole: string | undefined): boolean {
  return isAdmin(userRole);
}

// main.tsx
<Route
  path="/reports"
  element={
    <ProtectedRoute>
      <RoleProtectedRoute requiredRoles={["Admin"]}>
        <ReportsPage />
      </RoleProtectedRoute>
    </ProtectedRoute>
  }
/>;
```
