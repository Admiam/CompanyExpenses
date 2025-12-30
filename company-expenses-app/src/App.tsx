import { ThemeProvider } from "@/components/theme-provider";
import { Button } from "@/components/ui/button";
import { MainNavigationMenu } from "@/components/main-navigation-menu";
import { ModeToggle } from "@/components/node-toggle.tsx";
import { useAuth } from "@/auth/useAuth";

function App() {
  const { login } = useAuth();

  return (
    <ThemeProvider defaultTheme="dark" storageKey="vite-ui-theme">
      {/* Top Navigation */}
      <MainNavigationMenu />

      {/* Page content */}
      <div className="p-6">
        <div className="flex items-center justify-between mb-4">
          <h1 className="text-2xl font-bold">Welcome to Company Expenses!</h1>
          <ModeToggle />
        </div>

        <p className="mb-4">Systém pro správu firemních výdajů.</p>

        <Button onClick={login} size="lg">
          Přihlásit se
        </Button>
      </div>
    </ThemeProvider>
  );
}

export default App;
