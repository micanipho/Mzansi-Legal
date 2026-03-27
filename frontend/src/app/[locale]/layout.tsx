import type { Metadata } from "next";
import { NextIntlClientProvider } from "next-intl";
import { getMessages } from "next-intl/server";
import { notFound } from "next/navigation";
import { routing } from "@/i18n/routing";
import { AntdRegistry } from "@ant-design/nextjs-registry";
import AntdProvider from "@/components/providers/AntdProvider";
import AppNavbar from "@/components/layout/AppNavbar";
import OrganicBackground from "@/components/layout/OrganicBackground";
import "@/styles/globals.css";

export const metadata: Metadata = {
  title: "MzansiLegal — Know Your Rights",
  description:
    "AI-powered multilingual legal and financial rights assistant for South Africans",
};

export function generateStaticParams() {
  return routing.locales.map((locale) => ({ locale }));
}

export default async function LocaleLayout({
  children,
  params,
}: LayoutProps<"/[locale]">) {
  const { locale } = await params;

  if (!routing.locales.includes(locale as (typeof routing.locales)[number])) {
    notFound();
  }

  const messages = await getMessages();

  return (
    <html lang={locale}>
      <body>
        <a href="#main-content" className="skip-to-content">
          Skip to content
        </a>
        <NextIntlClientProvider messages={messages}>
          <AntdRegistry>
            <AntdProvider>
              <div
                style={{
                  position: "relative",
                  minHeight: "100vh",
                  width: "100%",
                  overflowX: "hidden",
                }}
              >
                <OrganicBackground />
                <AppNavbar />
                <div id="main-content">{children}</div>
              </div>
            </AntdProvider>
          </AntdRegistry>
        </NextIntlClientProvider>
      </body>
    </html>
  );
}
