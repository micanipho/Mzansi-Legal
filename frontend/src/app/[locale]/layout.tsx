import type { Metadata } from "next";
import { AntdRegistry } from "@ant-design/nextjs-registry";
import { NextIntlClientProvider } from "next-intl";
import { getMessages } from "next-intl/server";
import { notFound } from "next/navigation";
import ConditionalNav from "@/components/layout/ConditionalNav";
import OrganicBackground from "@/components/layout/OrganicBackground";
import AntdProvider from "@/components/providers/AntdProvider";
import AuthProvider from "@/components/providers/AuthProvider";
import { routing } from "@/i18n/routing";

export const metadata: Metadata = {
  title: "MzansiLegal - Know Your Rights",
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
    <>
      <a href="#main-content" className="skip-to-content">
        Skip to content
      </a>
      <NextIntlClientProvider messages={messages}>
        <AntdRegistry>
          <AntdProvider>
            <AuthProvider>
              <div className="app-shell">
                <OrganicBackground />
                <ConditionalNav />
                <div id="main-content" className="shell-main">
                  {children}
                </div>
              </div>
            </AuthProvider>
          </AntdProvider>
        </AntdRegistry>
      </NextIntlClientProvider>
    </>
  );
}
