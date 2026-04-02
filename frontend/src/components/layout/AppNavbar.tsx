"use client";

import { useEffect, useRef, useState } from "react";
import { Menu, Scale, X } from "lucide-react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { useLocale, useTranslations } from "next-intl";
import LocaleSwitcher from "@/components/layout/LocaleSwitcher";
import { useAuth } from "@/hooks/useAuth";
import { appRoutes, createLocalizedPath } from "@/i18n/routing";
import { C, fontSans, fontSerif, shadowOrganic } from "@/styles/theme";

const NAV_LINKS = [
  { labelKey: "home", path: appRoutes.home },
  { labelKey: "ask", path: appRoutes.ask },
  { labelKey: "contracts", path: appRoutes.contracts },
  { labelKey: "rights", path: appRoutes.rights },
  { labelKey: "history", path: appRoutes.history },
] as const;

function getUserInitials(name: string): string {
  const parts = name.trim().split(/\s+/).filter(Boolean);
  if (parts.length === 0) return "?";
  if (parts.length === 1) return parts[0][0].toUpperCase();
  return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
}

export default function AppNavbar() {
  const locale = useLocale();
  const pathname = usePathname();
  const tNav = useTranslations("nav");
  const tAuth = useTranslations("auth");
  const { user, signOut } = useAuth();

  const [menuOpen, setMenuOpen] = useState(false);
  const [dropdownOpen, setDropdownOpen] = useState(false);
  const navRef = useRef<HTMLElement>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      const target = event.target as Node;

      if (navRef.current && !navRef.current.contains(target)) {
        setMenuOpen(false);
        setDropdownOpen(false);
        return;
      }

      if (dropdownRef.current && !dropdownRef.current.contains(target)) {
        setDropdownOpen(false);
      }
    };

    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  useEffect(() => {
    const mediaQuery = window.matchMedia("(min-width: 901px)");

    const handleViewportChange = (event: MediaQueryListEvent) => {
      if (event.matches) {
        setMenuOpen(false);
      }
    };

    mediaQuery.addEventListener("change", handleViewportChange);
    return () => mediaQuery.removeEventListener("change", handleViewportChange);
  }, []);

  const isActive = (path: string) => {
    const full = createLocalizedPath(locale, path);
    if (path === "") {
      return pathname === full;
    }

    if (path === appRoutes.ask) {
      return (
        pathname === full ||
        pathname === createLocalizedPath(locale, appRoutes.legacyChat)
      );
    }

    return pathname.startsWith(full);
  };

  const desktopLinkStyle = (active: boolean) => ({
    padding: "8px 16px",
    borderRadius: 9999,
    fontSize: 14,
    fontWeight: 600,
    color: active ? C.fg : C.mutedFg,
    background: active ? C.muted : "transparent",
    textDecoration: "none",
    whiteSpace: "nowrap",
  });

  const mobileLinkStyle = (active: boolean) => ({
    display: "flex",
    width: "100%",
    minHeight: 44,
    alignItems: "center",
    justifyContent: "flex-start",
    padding: "12px 14px",
    borderRadius: 16,
    fontSize: 14,
    fontWeight: 700,
    color: active ? C.fg : C.mutedFg,
    background: active ? C.muted : "rgba(255,255,255,0.72)",
    border: `1px solid ${active ? "rgba(126, 107, 86, 0.26)" : C.border}`,
    textDecoration: "none",
  });

  const applyDesktopHover = (
    target: EventTarget & HTMLAnchorElement,
    active: boolean,
  ) => {
    target.style.background = active ? C.muted : "rgba(240, 235, 229, 0.82)";
    target.style.color = C.fg;
    target.style.transform = "translateY(-1px)";
  };

  const resetDesktopHover = (
    target: EventTarget & HTMLAnchorElement,
    active: boolean,
  ) => {
    target.style.background = active ? C.muted : "transparent";
    target.style.color = active ? C.fg : C.mutedFg;
    target.style.transform = "translateY(0)";
  };

  const applyMobileHover = (
    target: EventTarget & HTMLAnchorElement,
    active: boolean,
  ) => {
    target.style.background = active ? C.muted : "rgba(240, 235, 229, 0.86)";
    target.style.color = C.fg;
    target.style.borderColor = "rgba(126, 107, 86, 0.26)";
    target.style.transform = "translateY(-1px)";
  };

  const resetMobileHover = (
    target: EventTarget & HTMLAnchorElement,
    active: boolean,
  ) => {
    target.style.background = active ? C.muted : "rgba(255,255,255,0.72)";
    target.style.color = active ? C.fg : C.mutedFg;
    target.style.borderColor = active ? "rgba(126, 107, 86, 0.26)" : C.border;
    target.style.transform = "translateY(0)";
  };

  return (
    <header
      style={{
        position: "sticky",
        top: 16,
        zIndex: 50,
        padding: "0 16px",
        maxWidth: 1280,
        margin: "0 auto",
        width: "100%",
        pointerEvents: "none",
      }}
    >
      <nav
        ref={navRef}
        className="app-navbar-shell grain-panel"
        style={{
          pointerEvents: "auto",
          background: "rgba(255,255,255,0.72)",
          backdropFilter: "blur(12px)",
          WebkitBackdropFilter: "blur(12px)",
          border: "1px solid rgba(222,216,207,0.5)",
          borderRadius: 24,
          padding: "10px 16px",
          boxShadow: shadowOrganic,
          fontFamily: fontSans,
          overflow: "visible",
        }}
      >
        <div
          style={{
            display: "flex",
            alignItems: "center",
            justifyContent: "space-between",
            gap: 16,
            width: "100%",
          }}
        >
          <Link
            href={createLocalizedPath(locale, appRoutes.home)}
            onClick={() => setMenuOpen(false)}
            style={{
              display: "flex",
              alignItems: "center",
              gap: 12,
              textDecoration: "none",
            }}
          >
            <div
              style={{
                width: 40,
                height: 40,
                background: C.primary,
                borderRadius: 9999,
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                color: C.primaryFg,
                flexShrink: 0,
              }}
            >
              <Scale size={20} color={C.primaryFg} />
            </div>
            <span
              style={{
                fontFamily: fontSerif,
                fontWeight: 700,
                fontSize: 20,
                color: C.fg,
                letterSpacing: "-0.02em",
              }}
            >
              MzansiLegal
            </span>
          </Link>

          <div className="app-navbar-desktop">
            <div className="app-navbar-links">
              {NAV_LINKS.map(({ labelKey, path }) => {
                const active = isActive(path);
                return (
                  <Link
                    key={labelKey}
                    href={createLocalizedPath(locale, path)}
                    className="app-navbar-link"
                    aria-current={active ? "page" : undefined}
                    onMouseEnter={(event) =>
                      applyDesktopHover(event.currentTarget, active)
                    }
                    onMouseLeave={(event) =>
                      resetDesktopHover(event.currentTarget, active)
                    }
                    style={desktopLinkStyle(active)}
                  >
                    {tNav(labelKey)}
                  </Link>
                );
              })}
            </div>

            <div className="app-navbar-actions">
              <LocaleSwitcher
                buttonStyle={{
                  background: "transparent",
                  borderRadius: 9999,
                  fontSize: 13,
                  fontWeight: 600,
                  minHeight: 44,
                }}
              />

              {user ? (
                <div ref={dropdownRef} style={{ position: "relative" }}>
                  <button
                    onClick={() => setDropdownOpen((value) => !value)}
                    aria-haspopup="menu"
                    aria-expanded={dropdownOpen}
                    aria-label={`${user.name} - open menu`}
                    style={{
                      width: 44,
                      height: 44,
                      borderRadius: 9999,
                      background: C.primary,
                      color: C.primaryFg,
                      fontFamily: fontSerif,
                      fontWeight: 700,
                      fontSize: 15,
                      border: "none",
                      cursor: "pointer",
                      display: "flex",
                      alignItems: "center",
                      justifyContent: "center",
                      flexShrink: 0,
                    }}
                  >
                    {getUserInitials(user.name)}
                  </button>

                  {dropdownOpen ? (
                    <div
                      role="menu"
                      style={{
                        position: "absolute",
                        top: "calc(100% + 8px)",
                        right: 0,
                        minWidth: 200,
                        background: "#fff",
                        border: `1px solid ${C.border}`,
                        borderRadius: 16,
                        boxShadow: shadowOrganic,
                        overflow: "hidden",
                        zIndex: 100,
                      }}
                    >
                      {user.isAdmin ? (
                        <Link
                          href={createLocalizedPath(
                            locale,
                            appRoutes.adminDashboard,
                          )}
                          role="menuitem"
                          onClick={() => setDropdownOpen(false)}
                          style={{
                            display: "block",
                            padding: "12px 16px",
                            textDecoration: "none",
                            color: C.fg,
                            fontSize: 14,
                            fontWeight: 600,
                            fontFamily: fontSans,
                            borderBottom: `1px solid ${C.border}`,
                          }}
                        >
                          {tAuth("adminDashboardLink")}
                        </Link>
                      ) : null}
                      <button
                        role="menuitem"
                        onClick={() => {
                          setDropdownOpen(false);
                          signOut();
                        }}
                        style={{
                          display: "block",
                          width: "100%",
                          padding: "12px 16px",
                          textAlign: "left",
                          background: "none",
                          border: "none",
                          cursor: "pointer",
                          color: C.fg,
                          fontSize: 14,
                          fontWeight: 600,
                          fontFamily: fontSans,
                        }}
                      >
                        {tAuth("signOutMenuItem")}
                      </button>
                    </div>
                  ) : null}
                </div>
              ) : (
                <Link
                  href={createLocalizedPath(locale, appRoutes.auth)}
                  className="app-navbar-link"
                  style={{
                    background: C.primary,
                    color: C.primaryFg,
                    padding: "10px 24px",
                    borderRadius: 9999,
                    fontSize: 14,
                    fontWeight: 700,
                    textDecoration: "none",
                    fontFamily: fontSans,
                    whiteSpace: "nowrap",
                  }}
                >
                  {tNav("getStarted")}
                </Link>
              )}
            </div>
          </div>

          <button
            type="button"
            className="app-navbar-mobile-toggle"
            onClick={() => setMenuOpen((value) => !value)}
            aria-expanded={menuOpen}
            aria-controls="app-navbar-mobile-menu"
            aria-label={
              menuOpen ? "Close navigation menu" : "Open navigation menu"
            }
          >
            {menuOpen ? <X size={18} /> : <Menu size={18} />}
          </button>
        </div>

        {menuOpen ? (
          <div id="app-navbar-mobile-menu" className="app-navbar-mobile-menu">
            <div style={{ display: "grid", gap: 10 }}>
              {NAV_LINKS.map(({ labelKey, path }) => {
                const active = isActive(path);
                return (
                  <Link
                    key={labelKey}
                    href={createLocalizedPath(locale, path)}
                    onClick={() => setMenuOpen(false)}
                    aria-current={active ? "page" : undefined}
                    onMouseEnter={(event) =>
                      applyMobileHover(event.currentTarget, active)
                    }
                    onMouseLeave={(event) =>
                      resetMobileHover(event.currentTarget, active)
                    }
                    style={mobileLinkStyle(active)}
                  >
                    {tNav(labelKey)}
                  </Link>
                );
              })}
            </div>

            <div
              style={{
                display: "grid",
                gap: 10,
                paddingTop: 4,
              }}
            >
              <LocaleSwitcher
                buttonStyle={{
                  minHeight: 44,
                  width: "100%",
                  justifyContent: "space-between",
                  borderRadius: 16,
                  background: "rgba(255,255,255,0.72)",
                }}
                menuStyle={{ width: "100%" }}
              />

              {user ? (
                <>
                  {user.isAdmin ? (
                    <Link
                      href={createLocalizedPath(
                        locale,
                        appRoutes.adminDashboard,
                      )}
                      onClick={() => setMenuOpen(false)}
                      onMouseEnter={(event) =>
                        applyMobileHover(
                          event.currentTarget,
                          isActive(appRoutes.adminDashboard),
                        )
                      }
                      onMouseLeave={(event) =>
                        resetMobileHover(
                          event.currentTarget,
                          isActive(appRoutes.adminDashboard),
                        )
                      }
                      style={mobileLinkStyle(
                        isActive(appRoutes.adminDashboard),
                      )}
                    >
                      {tAuth("adminDashboardLink")}
                    </Link>
                  ) : null}
                  <button
                    type="button"
                    onClick={() => {
                      setMenuOpen(false);
                      signOut();
                    }}
                    style={{
                      minHeight: 44,
                      width: "100%",
                      padding: "12px 14px",
                      borderRadius: 16,
                      border: `1px solid ${C.border}`,
                      background: "rgba(255,255,255,0.72)",
                      color: C.fg,
                      textAlign: "left",
                      fontWeight: 700,
                      cursor: "pointer",
                    }}
                  >
                    {tAuth("signOutMenuItem")}
                  </button>
                </>
              ) : (
                <Link
                  href={createLocalizedPath(locale, appRoutes.auth)}
                  onClick={() => setMenuOpen(false)}
                  style={{
                    display: "flex",
                    width: "100%",
                    minHeight: 44,
                    alignItems: "center",
                    justifyContent: "center",
                    padding: "12px 14px",
                    borderRadius: 16,
                    background: C.primary,
                    color: C.primaryFg,
                    textDecoration: "none",
                    fontWeight: 700,
                  }}
                >
                  {tNav("getStarted")}
                </Link>
              )}
            </div>
          </div>
        ) : null}
      </nav>
    </header>
  );
}
