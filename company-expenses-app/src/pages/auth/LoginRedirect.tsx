import { useEffect } from "react";
import { AUTH_CONFIG } from "@/lib/auth-config";

export default function LoginRedirect() {
  useEffect(() => {
    const returnUrl = encodeURIComponent(window.location.origin + "/dashboard");
    window.location.href = `${AUTH_CONFIG.authServerUrl}/Account/Login?returnUrl=${returnUrl}`;
  }, []);

  return (
    <div className="flex min-h-screen items-center justify-center">
      <div className="text-center">
        <div className="mb-4 inline-block h-8 w-8 animate-spin rounded-full border-4 border-solid border-current border-r-transparent"></div>
        <p className="text-muted-foreground">Přesměrování na přihlášení...</p>
      </div>
    </div>
  );
}
