import type { ReactNode } from 'react';

interface Column<T> {
  header: string;
  accessor: (row: T) => ReactNode;
  className?: string;
}

interface TableProps<T> {
  columns: Column<T>[];
  data: T[];
  keyExtractor: (row: T) => string;
  emptyMessage?: string;
}

export function Table<T>({ columns, data, keyExtractor, emptyMessage = 'Nenhum resultado encontrado.' }: TableProps<T>) {
  if (data.length === 0) {
    return <p className="text-gray-500 text-center py-4">{emptyMessage}</p>;
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b text-left">
            {columns.map((col) => (
              <th key={col.header} className={`py-2 font-medium text-gray-600 ${col.className || ''}`}>{col.header}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {data.map((row) => (
            <tr key={keyExtractor(row)} className="border-b hover:bg-gray-50">
              {columns.map((col) => (
                <td key={col.header} className={`py-2 ${col.className || ''}`}>{col.accessor(row)}</td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
