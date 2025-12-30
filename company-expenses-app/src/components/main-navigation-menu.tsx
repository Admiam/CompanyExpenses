"use client"

import { Link } from "react-router-dom"
import { LogIn } from "lucide-react"
import { Button } from "@/components/ui/button"

export function MainNavigationMenu() {
    return (
        <div className="flex w-full items-center justify-between px-4 py-2 border-b">
            {/* Left side - Logo */}
            <div className="font-bold text-lg">
                <Link to="/">Company Expenses</Link>
            </div>

            {/* Right side - Login button */}
            <div>
                <Button asChild variant="outline" size="sm">
                    <Link to="/login">
                        <LogIn className="mr-2 h-4 w-4" />
                        Login
                    </Link>
                </Button>
            </div>
        </div>
    )
}
