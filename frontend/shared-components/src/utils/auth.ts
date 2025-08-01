import { getServerSession } from "next-auth";
import { authOptions } from "@/lib/auth/auth-options";

export const getSession = () => getServerSession(authOptions);
export { authOptions };