import type { ReactNode } from 'react';

interface CardProps {
  title?: string;
  children: ReactNode;
  className?: string;
}

export function Card({ title, children, className = '' }: CardProps) {
  return (
    <div className={`bg-white rounded-xl border border-gray-200 shadow-sm ${className}`}>
      {title && <div className="px-6 py-4 border-b border-gray-200 font-semibold text-gray-800">{title}</div>}
      <div className="p-6">{children}</div>
    </div>
  );
}
