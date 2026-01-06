import * as React from "react";
import { IconDashboard, IconUsers, IconSettings, IconCreditCard, IconBuildingSkyscraper, IconHelp, IconCategory } from "@tabler/icons-react";

import { NavMain } from "@/components/nav-main";
import { NavSecondary } from "@/components/nav-secondary";
import { NavUser } from "@/components/nav-user";
import { Sidebar, SidebarContent, SidebarFooter, SidebarHeader, SidebarMenu, SidebarMenuButton, SidebarMenuItem } from "@/components/ui/sidebar";
import { useAuth } from "@/auth/useAuth";
import { canAccessWorkplaces, canAccessUsers, canAccessCategories } from "@/utils/roles";

const data = {
  user: {
    name: "Admin User",
    email: "admin@company.com",
    avatar: "/avatars/admin.jpg",
  },
  navMain: [
    {
      title: "Dashboard",
      url: "/dashboard",
      icon: IconDashboard,
    },
    {
      title: "Výdaje",
      url: "/expenses",
      icon: IconCreditCard,
    },
    {
      title: "Pracoviště",
      url: "/workplaces",
      icon: IconBuildingSkyscraper,
    },
    {
      title: "Uživatelé",
      url: "/users",
      icon: IconUsers,
    },
    {
      title: "Kategorie",
      url: "/categories",
      icon: IconCategory,
    },
  ],
  navSecondary: [
    {
      title: "Nastavení",
      url: "/settings",
      icon: IconSettings,
    },
    {
      title: "Nápověda",
      url: "/help",
      icon: IconHelp,
    },
  ],
};

export function AppSidebar({ ...props }: React.ComponentProps<typeof Sidebar>) {
  const { user } = useAuth();
  const userRole = user?.role;

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
    // Dashboard and Expenses are accessible to all authenticated users
    return true;
  });

  return (
    <Sidebar collapsible="offcanvas" {...props}>
      <SidebarHeader>
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton asChild className="data-[slot=sidebar-menu-button]:!p-1.5">
              <a href="/">
                <span className="text-base font-semibold">Company Expenses</span>
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
            name: user?.name || "Uživatel",
            email: user?.email || "",
            avatar: "/avatars/admin.jpg",
            role: user?.role || "User",
          }}
        />
      </SidebarFooter>
    </Sidebar>
  );
}
