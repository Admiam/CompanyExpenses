import * as React from "react";
import { IconDashboard, IconUsers, IconSettings, IconCreditCard, IconBuildingSkyscraper, IconHelp, IconCategory } from "@tabler/icons-react";
import { useTranslation } from "react-i18next";

import { NavMain } from "@/components/nav-main";
import { NavSecondary } from "@/components/nav-secondary";
import { NavUser } from "@/components/nav-user";
import { Sidebar, SidebarContent, SidebarFooter, SidebarHeader, SidebarMenu, SidebarMenuButton, SidebarMenuItem } from "@/components/ui/sidebar";
import { useAuth } from "@/auth/useAuth";
import { canAccessWorkplaces, canAccessUsers, canAccessCategories } from "@/utils/roles";

const getNavData = (t: (key: string) => string) => ({
  user: {
    name: "Admin User",
    email: "admin@company.com",
    avatar: "/avatars/admin.jpg",
  },
  navMain: [
    {
      title: t("nav.dashboard"),
      url: "/dashboard",
      icon: IconDashboard,
    },
    {
      title: t("nav.expenses"),
      url: "/expenses",
      icon: IconCreditCard,
    },
    {
      title: t("nav.workplaces"),
      url: "/workplaces",
      icon: IconBuildingSkyscraper,
    },
    {
      title: t("nav.users"),
      url: "/users",
      icon: IconUsers,
    },
    {
      title: t("nav.categories"),
      url: "/categories",
      icon: IconCategory,
    },
  ],
  navSecondary: [
    {
      title: t("nav.settings"),
      url: "/settings",
      icon: IconSettings,
    },
    {
      title: t("nav.help"),
      url: "/help",
      icon: IconHelp,
    },
  ],
});

export function AppSidebar({ ...props }: React.ComponentProps<typeof Sidebar>) {
  const { user } = useAuth();
  const { t } = useTranslation();
  const userRole = user?.role;
  const data = getNavData(t);

  // Filter navigation items based on user role
  const navMainFiltered = data.navMain.filter((item) => {
    if (item.url === "/workplaces") {
      return canAccessWorkplaces(userRole);
    }
    if (item.url === "/users") {
      return canAccessUsers(userRole);
    }
    if (item.url === "/categories") {
      return canAccessCategories(userRole);
    }
    return true;
  });

  return (
    <Sidebar collapsible="offcanvas" {...props}>
      <SidebarHeader>
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton asChild className="data-[slot=sidebar-menu-button]:!p-1.5">
              <a href="/">
                <span className="text-base font-semibold">{t("app.title")}</span>
              </a>
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarHeader>
      <SidebarContent>
        <NavMain items={navMainFiltered} />
        <NavSecondary items={data.navSecondary} className="mt-auto" />
      </SidebarContent>
      <SidebarFooter>
        <NavUser
          user={{
            name: user?.name || t("users.user"),
            email: user?.email || "",
            avatar: "/avatars/admin.jpg",
            role: user?.role || "User",
          }}
        />
      </SidebarFooter>
    </Sidebar>
  );
}
