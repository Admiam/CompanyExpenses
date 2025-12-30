"use client"

import { useState } from "react"
import { cn } from "@/lib/utils"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import {
    InputOTP,
    InputOTPGroup,
    InputOTPSeparator,
    InputOTPSlot,
} from "@/components/ui/input-otp"
import {useNavigate} from "react-router-dom";

export function ForgotPasswordForm({
                                       className,
                                       ...props
                                   }: React.ComponentProps<"div">) {
    const [step, setStep] = useState<"email" | "verify">("email")
    const [email, setEmail] = useState("")
    const [code, setCode] = useState("")

    const navigate = useNavigate()

    const handleSendCode = (e: React.FormEvent) => {
        e.preventDefault()
        // TODO: call backend API to send verification code
        console.log("Send code to:", email)
        setStep("verify")
    }

    const handleVerifyCode = (e: React.FormEvent) => {
        e.preventDefault()
        // TODO: verify code with backend
        console.log("Verify code:", code)
        alert("Code verified! Now you can reset your password.")
        navigate("/reset-password")
    }

    return (
        <div className={cn("flex flex-col gap-6", className)} {...props}>
            <Card>
                <CardContent className="p-6">
                    {step === "email" && (
                        <form className="flex flex-col gap-6" onSubmit={handleSendCode}>
                            <div className="text-center">
                                <h1 className="text-2xl font-bold">Forgot Password</h1>
                                <p className="text-muted-foreground mt-1 text-sm">
                                    Enter your email to receive a verification code
                                </p>
                            </div>
                            <div className="grid gap-3">
                                <Label htmlFor="email">Email</Label>
                                <Input
                                    id="email"
                                    type="email"
                                    placeholder="m@example.com"
                                    value={email}
                                    onChange={(e) => setEmail(e.target.value)}
                                    required
                                />
                            </div>
                            <Button type="submit" className="w-full">
                                Send Code
                            </Button>
                        </form>
                    )}

                    {step === "verify" && (
                        <form className="flex flex-col gap-6 items-center justify-center" onSubmit={handleVerifyCode}>
                            <div className="text-center">
                                <h1 className="text-2xl font-bold">Verify Code</h1>
                                <p className="text-muted-foreground mt-1 text-sm">
                                    We sent a 6-digit code to <b>{email}</b>
                                </p>
                            </div>
                            <InputOTP
                                maxLength={6}
                                value={code}
                                onChange={(val) => setCode(val)}
                            >
                                <InputOTPGroup>
                                    <InputOTPSlot index={0} />
                                    <InputOTPSlot index={1} />
                                    <InputOTPSlot index={2} />
                                </InputOTPGroup>
                                <InputOTPSeparator />
                                <InputOTPGroup>
                                    <InputOTPSlot index={3} />
                                    <InputOTPSlot index={4} />
                                    <InputOTPSlot index={5} />
                                </InputOTPGroup>
                            </InputOTP>
                            <Button type="submit" className="w-full">
                                Verify
                            </Button>
                        </form>
                    )}
                </CardContent>
            </Card>
        </div>
    )
}
