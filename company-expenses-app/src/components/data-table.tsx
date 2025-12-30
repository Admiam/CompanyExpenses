import * as React from "react"
import {
  type ColumnDef,
  type ColumnFiltersState,
  flexRender,
  getCoreRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  getSortedRowModel,
  type SortingState,
  useReactTable,
  type VisibilityState,
} from "@tanstack/react-table"
import { toast } from "sonner"
import { z } from "zod"

import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Checkbox } from "@/components/ui/checkbox"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"

// eslint-disable-next-line react-refresh/only-export-components
export const schema = z.object({
  id: z.number(),
  employee: z.string(),
  description: z.string(),
  amount: z.number(),
  date: z.string(),
  status: z.string()
})

type Request = z.infer<typeof schema>

const columns: ColumnDef<Request>[] = [
  {
    id: "select",
    header: ({ table }) => (
        <div className="flex items-center justify-center">
          <Checkbox
              checked={
                  table.getIsAllPageRowsSelected() ||
                  (table.getIsSomePageRowsSelected() && "indeterminate")
              }
              onCheckedChange={(value) => table.toggleAllPageRowsSelected(!!value)}
              aria-label="Select all"
          />
        </div>
    ),
    cell: ({ row }) => (
        <div className="flex items-center justify-center">
          <Checkbox
              checked={row.getIsSelected()}
              onCheckedChange={(value) => row.toggleSelected(!!value)}
              aria-label="Select row"
          />
        </div>
    ),
    enableSorting: false,
    enableHiding: false,
  },
  {
    accessorKey: "employee",
    header: "Zaměstnanec", // ✅ keep UI text Czech
  },
  {
    accessorKey: "description",
    header: "Popis",
  },
  {
    accessorKey: "amount",
    header: "Částka",
    cell: ({ row }) => (
        <span>{row.original.amount.toLocaleString("cs-CZ")} Kč</span>
    ),
  },
  {
    accessorKey: "date",
    header: "Datum",
    cell: ({ row }) =>
        new Date(row.original.date).toLocaleDateString("cs-CZ"),
  },
  {
    accessorKey: "status",
    header: "Stav",
    cell: ({ row }) => (
        <Badge
            variant={
              row.original.status === "Rejected"
                  ? "destructive"
                  : row.original.status === "Pending"
                      ? "outline"
                      : "secondary"
            }
            className={
              row.original.status === "Approved"
                  ? "bg-green-500 text-white hover:bg-green-600"
                  : ""
            }
        >
          {/* UI text stays Czech */}
          {row.original.status === "Rejected" && "Zamítnuto"}
          {row.original.status === "Pending" && "Čeká"}
          {row.original.status === "Approved" && "Schváleno"}
        </Badge>
    ),
  },
  {
    id: "actions",
    header: "Akce",
    cell: ({ row }) => (
        <div className="flex gap-2">
          <Button
              size="sm"
              variant="outline"
              onClick={() => {
                toast.success(`Žádost #${row.original.id} schválena`)
              }}
          >
            Přijmout
          </Button>
          <Button
              size="sm"
              variant="destructive"
              onClick={() => {
                toast.error(`Žádost #${row.original.id} zamítnuta`)
              }}
          >
            Odmítnout
          </Button>
        </div>
    ),
  },
]


// --- Hlavní komponenta
export function DataTable({
                            data: initialData,
                          }: {
  data: Request[]
}) {
  const [data] = React.useState(() => initialData)
  const [rowSelection, setRowSelection] = React.useState({})
  const [columnVisibility, setColumnVisibility] =
      React.useState<VisibilityState>({})
  const [columnFilters, setColumnFilters] =
      React.useState<ColumnFiltersState>([])
  const [sorting, setSorting] = React.useState<SortingState>([])
  const [pagination, setPagination] = React.useState({
    pageIndex: 0,
    pageSize: 10,
  })

  const table = useReactTable({
    data,
    columns,
    state: {
      sorting,
      columnVisibility,
      rowSelection,
      columnFilters,
      pagination,
    },
    getRowId: (row) => row.id.toString(),
    enableRowSelection: true,
    onRowSelectionChange: setRowSelection,
    onSortingChange: setSorting,
    onColumnFiltersChange: setColumnFilters,
    onColumnVisibilityChange: setColumnVisibility,
    onPaginationChange: setPagination,
    getCoreRowModel: getCoreRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    getSortedRowModel: getSortedRowModel(),
  })

  return (
      <div className="overflow-hidden rounded-lg border">
        <Table>
          <TableHeader className="bg-muted sticky top-0 z-10">
            {table.getHeaderGroups().map((headerGroup) => (
                <TableRow key={headerGroup.id}>
                  {headerGroup.headers.map((header) => (
                      <TableHead key={header.id} colSpan={header.colSpan}>
                        {header.isPlaceholder
                            ? null
                            : flexRender(
                                header.column.columnDef.header,
                                header.getContext()
                            )}
                      </TableHead>
                  ))}
                </TableRow>
            ))}
          </TableHeader>
          <TableBody>
            {table.getRowModel().rows?.length ? (
                table.getRowModel().rows.map((row) => (
                    <TableRow
                        key={row.id}
                        data-state={row.getIsSelected() && "selected"}
                    >
                      {row.getVisibleCells().map((cell) => (
                          <TableCell key={cell.id}>
                            {flexRender(cell.column.columnDef.cell, cell.getContext())}
                          </TableCell>
                      ))}
                    </TableRow>
                ))
            ) : (
                <TableRow>
                  <TableCell colSpan={columns.length} className="h-24 text-center">
                    Žádné výsledky.
                  </TableCell>
                </TableRow>
            )}
          </TableBody>
        </Table>
      </div>
  )
}
