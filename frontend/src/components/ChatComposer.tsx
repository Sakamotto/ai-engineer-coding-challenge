import type { FormEvent, KeyboardEvent } from 'react'

interface ChatComposerProps {
  value: string
  onChange: (value: string) => void
  onSubmit: () => void
  isBusy: boolean
}

export function ChatComposer({ value, onChange, onSubmit, isBusy }: ChatComposerProps) {
  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    onSubmit()
  }

  function handleKeyDown(event: KeyboardEvent<HTMLTextAreaElement>) {
    if (event.key === 'Enter' && (event.ctrlKey || event.metaKey)) {
      event.preventDefault()
      if (!isBusy && value.trim().length > 0) {
        onSubmit()
      }
    }
  }

  return (
    <form className="flex flex-col gap-3 p-4 border-t bg-background" onSubmit={handleSubmit}>
      <label htmlFor="chat-input" className="sr-only">Ask about the grocery store SOP</label>
      <textarea
        id="chat-input"
        rows={3}
        value={value}
        onKeyDown={handleKeyDown}
        onChange={(event) => onChange(event.target.value)}
        placeholder="Try: What are the opening checklist steps?"
        disabled={isBusy}
        className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 resize-y"
      />
      <div className="flex justify-between items-center text-sm">
        <span className="text-muted-foreground">Press <kbd className="px-1 py-0.5 rounded-sm bg-muted text-xs font-mono">Ctrl + Enter</kbd> to send message</span>
        <button 
          className="inline-flex items-center justify-center whitespace-nowrap rounded-md text-sm font-medium ring-offset-background transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50 bg-primary text-primary-foreground hover:bg-primary/90 h-9 px-4 py-2"
          type="submit" 
          disabled={isBusy || value.trim().length === 0}
        >
          {isBusy ? 'Sending...' : 'Send'}
        </button>
      </div>
    </form>
  )
}