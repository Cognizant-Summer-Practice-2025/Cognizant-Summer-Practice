import NextAuth from "next-auth"
import { authOptions } from "@cognizant-summer-practice/shared-components"

const handler = NextAuth(authOptions)

export { handler as GET, handler as POST } 