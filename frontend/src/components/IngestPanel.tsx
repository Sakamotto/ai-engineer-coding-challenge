import type { FormEvent } from 'react'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Database } from 'lucide-react'

interface IngestPanelProps {
  sourcePath: string
  onSourcePathChange: (value: string) => void
  onIngest: () => void
  isBusy: boolean
}

export function IngestPanel({ sourcePath, onSourcePathChange, onIngest, isBusy }: IngestPanelProps) {
  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    onIngest()
  }

  return (
    <Card className="shadow-sm border-0 border-t-4 border-t-primary/20 bg-card/60 backdrop-blur">
      <CardHeader className="pb-3">
        <CardTitle className="text-sm font-semibold flex items-center gap-2">
           <Database className="w-4 h-4 text-primary" /> Data Knowledge Base
        </CardTitle>
        <CardDescription className="text-xs">
          Populate the local Vector Store by chunking and embedding the provided markdown file.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form className="flex flex-col gap-3" onSubmit={handleSubmit}>
          <div className="flex flex-col gap-1.5">
            <label htmlFor="source-path" className="text-xs font-medium px-1">Source Path (Local File)</label>
            <input
              id="source-path"
              className="flex h-9 w-full rounded-md border border-input bg-background/50 px-3 py-1 text-xs shadow-sm transition-colors file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
              value={sourcePath}
              onChange={(event) => onSourcePathChange(event.target.value)}
              disabled={isBusy}
              spellCheck="false"
            />
          </div>
          <button className="inline-flex items-center justify-center rounded-md text-xs font-medium transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:pointer-events-none disabled:opacity-50 border border-input bg-background shadow-sm hover:bg-accent hover:text-accent-foreground h-8 px-3 mt-1" type="submit" disabled={isBusy || sourcePath.trim().length === 0}>
            {isBusy ? 'Chunking and Embedding...' : 'Run Ingest'}
          </button>
        </form>
      </CardContent>
    </Card>
  )
}