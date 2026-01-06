import { ThemeProvider } from "@/components/theme-provider";
import { Button } from "@/components/ui/button";
import { MainNavigationMenu } from "@/components/main-navigation-menu";
import { ModeToggle } from "@/components/node-toggle.tsx";
import { LanguageToggle } from "@/components/language-toggle.tsx";
import { useAuth } from "@/auth/useAuth";
import { useTranslation } from "react-i18next";

function App() {
  const { login } = useAuth();
  const { t } = useTranslation();

  return (
    <ThemeProvider defaultTheme="dark" storageKey="vite-ui-theme">
      {/* Top Navigation */}
      <MainNavigationMenu />

      {/* Page content */}
      <div className="p-6">
        <div className="flex items-center justify-between mb-4">
          <h1 className="text-2xl font-bold">{t("app.welcome")}</h1>
          <div className="flex items-center gap-2">
            <LanguageToggle />
            <ModeToggle />
          </div>
        </div>

        <p className="mb-4">{t("app.description")}</p>

        <Button onClick={login} size="lg">
          {t("app.login")}
        </Button>
      </div>
    </ThemeProvider>
  );
}

export default App;
