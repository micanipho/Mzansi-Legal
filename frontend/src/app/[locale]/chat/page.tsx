import { redirect } from "next/navigation";
import { appRoutes, createLocalizedPath } from "@/i18n/routing";

interface LegacyChatPageProps {
  params: Promise<{ locale: string }>;
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}

function createQueryString(searchParams: Record<string, string | string[] | undefined>): string {
  const query = new URLSearchParams();

  Object.entries(searchParams).forEach(([key, value]) => {
    if (Array.isArray(value)) {
      value.forEach((item) => query.append(key, item));
      return;
    }

    if (value) {
      query.set(key, value);
    }
  });

  return query.toString();
}

export default async function LegacyChatPage({ params, searchParams }: LegacyChatPageProps) {
  const { locale } = await params;
  const resolvedSearchParams = await searchParams;
  const queryString = createQueryString(resolvedSearchParams);

  redirect(createLocalizedPath(locale, appRoutes.ask, queryString));
}
