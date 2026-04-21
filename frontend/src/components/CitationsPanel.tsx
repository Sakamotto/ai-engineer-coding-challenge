import type { Citation } from '../types/chat'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'

interface CitationsPanelProps {
  citations: Citation[]
}

export function CitationsPanel({ citations }: CitationsPanelProps) {
  if (citations.length === 0) {
    return (
      <Card className="shadow-none border-dashed bg-transparent mt-4">
        <CardContent className="p-6 text-center text-muted-foreground text-sm">
          No citations available for the last interaction.
        </CardContent>
      </Card>
    )
  }

  return (
    <div className="space-y-4">
      <h3 className="font-semibold text-sm -mb-2">References ({citations.length})</h3>
      {citations.map((citation, index) => (
        <Card key={index} className="overflow-hidden shadow-sm">
          <CardHeader className="py-3 px-4 bg-muted/40 border-b">
             <CardTitle className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">Source</CardTitle>
             <p className="text-sm font-medium leading-tight">{citation.source}</p>
          </CardHeader>
          <CardContent className="p-4 bg-card">
            <p className="text-xs text-muted-foreground italic">&ldquo;{citation.snippet}&rdquo;</p>
          </CardContent>
        </Card>
      ))}
    </div>
  )
}