"use client";

import { Suspense, useEffect, useMemo } from "react";
import { useSearchParams } from "next/navigation";
import { LoadingOverlay } from "@/components/loader/loading-overlay";

export const dynamic = 'force-dynamic';

function SigningOutContent() {
  const searchParams = useSearchParams();

  const { services, returnUrl } = useMemo(() => {
    const rawServices = (searchParams.get("services") || "")
      .split(",")
      .map((s) => s.trim())
      .filter((s) => s.startsWith("http://") || s.startsWith("https://"));
    const ret = searchParams.get("return") || "/";
    return { services: rawServices, returnUrl: ret };
  }, [searchParams]);

  useEffect(() => {
    let cancelled = false;

    const finish = () => {
      if (cancelled) return;
      try {
        const f = new URL(returnUrl, window.location.origin);
        f.searchParams.set("signout", "1");
        window.location.replace(f.toString());
      } catch {
        window.location.replace(returnUrl);
      }
    };

    const loadService = (i: number) => {
      if (cancelled) return;
      if (i >= services.length) {
        finish();
        return;
      }
      try {
        const u = new URL(services[i]);
        u.searchParams.set("signout", "1");
        const iframe = document.createElement("iframe");
        iframe.style.display = "none";
        iframe.referrerPolicy = "no-referrer";
        iframe.src = u.toString();
        let done = false;
        const proceed = () => {
          if (done || cancelled) return;
          done = true;
          setTimeout(() => {
            try { document.body.removeChild(iframe); } catch {}
            loadService(i + 1);
          }, 200);
        };
        iframe.onload = proceed;
        setTimeout(proceed, 1200);
        document.body.appendChild(iframe);
      } catch {
        loadService(i + 1);
      }
    };

    loadService(0);

    return () => { cancelled = true; };
  }, [services, returnUrl]);

  return (
    <div className="min-h-screen w-full bg-[#0b0f1a]">
      <LoadingOverlay
        isOpen={true}
        title="Signing out from all services…"
        message="Please wait a moment"
        showBackdrop={false}
        textColor="#e5e7eb"
        backgroundColor="#0b0f1a"
      />
    </div>
  );
}

export default function SigningOutPage() {
  return (
    <Suspense fallback={
      <div className="min-h-screen w-full bg-[#0b0f1a]">
        <LoadingOverlay isOpen={true} title="Signing out…" showBackdrop={false} textColor="#e5e7eb" backgroundColor="#0b0f1a" />
      </div>
    }>
      <SigningOutContent />
    </Suspense>
  );
}


