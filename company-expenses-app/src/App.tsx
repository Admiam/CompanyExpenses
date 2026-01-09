import { ThemeProvider } from "@/components/theme-provider";
import { Button } from "@/components/ui/button";
import { useTranslation } from "react-i18next";
import { LogIn } from "lucide-react";

function App() {
  const { t } = useTranslation();

  const handleLogin = () => {
    const returnUrl = encodeURIComponent(window.location.origin + "/dashboard");
    window.location.href = `http://localhost:7169/Account/Login?returnUrl=${returnUrl}`;
  };

  return (
    <ThemeProvider defaultTheme="dark" storageKey="vite-ui-theme">
      <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-gray-50 to-gray-100 dark:from-gray-900 dark:to-gray-800">
        <div className="w-full max-w-4xl mx-auto px-4">
          <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-xl overflow-hidden">
            <div className="grid md:grid-cols-2 gap-0">
              {/* Content Section */}
              <div className="p-8 md:p-12 flex flex-col justify-center">
                <div className="text-center mb-8">
                  <div className="inline-flex items-center justify-center w-20 h-20 bg-blue-100 dark:bg-blue-900 rounded-full mb-6">
                    <LogIn className="w-10 h-10 text-blue-600 dark:text-blue-400" />
                  </div>
                  <h1 className="text-4xl font-bold text-gray-900 dark:text-white mb-4">
                    {t("app.welcome")}
                  </h1>
                  <p className="text-lg text-gray-600 dark:text-gray-300">
                    {t("app.description")}
                  </p>
                </div>

                <div className="space-y-4">
                  <Button 
                    onClick={handleLogin} 
                    size="lg" 
                    className="w-full bg-blue-600 hover:bg-blue-700 text-white"
                  >
                    <LogIn className="mr-2 h-5 w-5" />
                    {t("app.login")}
                  </Button>

                  <p className="text-center text-sm text-gray-500 dark:text-gray-400">
                    Přihlaste se pro správu firemních výdajů
                  </p>
                </div>
              </div>

              {/* Image/Gradient Section */}
              <div className="hidden md:block relative bg-gradient-to-br from-blue-600 to-purple-700">
                <div className="absolute inset-0 bg-black bg-opacity-20"></div>
                <div className="absolute inset-0 flex items-center justify-center p-12">
                  <div className="text-white text-center">
                    <h2 className="text-3xl font-bold mb-4">Company Expenses</h2>
                    <p className="text-lg opacity-90">
                      Správa výdajů pro vaši firmu
                    </p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </ThemeProvider>
  );
}

export default App;
