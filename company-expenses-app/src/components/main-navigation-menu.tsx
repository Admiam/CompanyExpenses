"use client"

import { Link } from "react-router-dom"
import { LogIn } from "lucide-react"
import { Button } from "@/components/ui/button"
import { AUTH_CONFIG } from "@/lib/auth-config"

export function MainNavigationMenu() {
    const loginUrl = `${AUTH_CONFIG.authServerUrl}/Account/Login?returnUrl=${encodeURIComponent(window.location.origin + "/dashboard")}`;
    
    return (
        <div className="flex w-full items-center justify-between px-4 py-2 border-b">
            {/* Left side - Logo */}
            <div className="font-bold text-lg">
                <Link to="/">Company Expenses</Link>
            </div>

            {/* Right side - Login button */}
            <div>
                <Button asChild variant="outline" size="sm">
                    <a href={loginUrl}>
                        <LogIn className="mr-2 h-4 w-4" />
                        Login
                    </a>
                </Button>
            </div>
        </div>
    )
}
