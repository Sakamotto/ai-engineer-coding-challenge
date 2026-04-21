import type { StatusMessage } from '../types/chat'
import { Info, CheckCircle, AlertTriangle, AlertCircle } from 'lucide-react'

interface StatusBannerProps {
  status: StatusMessage
}

export function StatusBanner({ status }: StatusBannerProps) {
  const tones = {
    info: 'bg-blue-50 text-blue-900 border-blue-200 dark:bg-blue-950/40 dark:text-blue-300 dark:border-blue-900',
    success: 'bg-green-50 text-green-900 border-green-200 dark:bg-green-950/40 dark:text-green-300 dark:border-green-900',
    warning: 'bg-yellow-50 text-yellow-900 border-yellow-200 dark:bg-yellow-950/40 dark:text-yellow-300 dark:border-yellow-900',
    error: 'bg-red-50 text-red-900 border-red-200 dark:bg-red-950/40 dark:text-red-300 dark:border-red-900',
  }

  const icons = {
    info: <Info className="w-4 h-4" />,
    success: <CheckCircle className="w-4 h-4" />,
    warning: <AlertTriangle className="w-4 h-4" />,
    error: <AlertCircle className="w-4 h-4" />,
  }

  return (
    <div className={`px-4 py-2 border-b flex items-center gap-3 text-xs font-medium transition-colors ${tones[status.tone]}`}>
      {icons[status.tone]}
      <span>{status.message}</span>
    </div>
  )
}