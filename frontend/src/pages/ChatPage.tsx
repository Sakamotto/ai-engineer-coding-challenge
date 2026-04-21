import { useEffect, useState } from 'react'
import { apiClient } from '../services/apiClient'
import type { ChatMessage, Citation, StatusMessage } from '../types/chat'
import { ChatComposer } from '../components/ChatComposer'
import { ChatTranscript } from '../components/ChatTranscript'
import { CitationsPanel } from '../components/CitationsPanel'
import { IngestPanel } from '../components/IngestPanel'
import { StatusBanner } from '../components/StatusBanner'

const defaultSourcePath = '../../../../knowledge-base/Grocery_Store_SOP.md'

function createMessage(role: ChatMessage['role'], content: string): ChatMessage {
  return {
    id: window.crypto.randomUUID(),
    role,
    content,
    timestamp: new Date().toISOString(),
  }
}

export function ChatPage() {
  const [conversationId] = useState(() => window.crypto.randomUUID())
  const [draft, setDraft] = useState('')
  const [sourcePath, setSourcePath] = useState(defaultSourcePath)
  const [isSending, setIsSending] = useState(false)
  const [isIngesting, setIsIngesting] = useState(false)
  const [citations, setCitations] = useState<Citation[]>([])
  const [status, setStatus] = useState<StatusMessage>({
    tone: 'info',
    message: 'Checking backend health...',
  })
  const [messages, setMessages] = useState<ChatMessage[]>(() => {
    return [
      createMessage(
        'assistant',
        'Hello! I am your AI Assistant. I can help search the operations manual in our knowledge base. To begin, click "Run Ingest"!',
      ),
    ]
  })

  useEffect(() => {
    let isCancelled = false

    async function loadHealth() {
      try {
        const health = await apiClient.getHealth()

        if (!isCancelled) {
          setStatus({
            tone: 'success',
            message: `${health.service} is running. ${health.notes[0] ?? ''}`.trim(),
          })
        }
      } catch (error) {
        if (!isCancelled) {
          setStatus({
            tone: 'warning',
            message: error instanceof Error
              ? `Backend health check failed: ${error.message}`
              : 'Backend health check failed.',
          })
        }
      }
    }

    void loadHealth()

    return () => {
      isCancelled = true
    }
  }, [])

  async function handleIngest() {
    setIsIngesting(true)
    setStatus({ tone: 'info', message: 'Calling the ingest endpoint...' })

    try {
      const response = await apiClient.ingest({
        sourcePath,
        forceReingest: false,
      })

      setStatus({
        tone: response.isPlaceholder ? 'warning' : 'success',
        message: `${response.message}`,
      })
    } catch (error) {
      setStatus({
        tone: 'error',
        message: error instanceof Error ? error.message : 'Ingest request failed.',
      })
    } finally {
      setIsIngesting(false)
    }
  }

  async function handleSend() {
    const trimmedDraft = draft.trim()
    if (!trimmedDraft) return

    const userMessage = createMessage('user', trimmedDraft)
    const nextMessages = [...messages, userMessage]

    setMessages(nextMessages)
    setDraft('')
    setIsSending(true)
    setStatus({ tone: 'info', message: 'Sending chat request...' })

    try {
      const response = await apiClient.chat({
        conversationId,
        useTools: true,
        messages: nextMessages.map((message) => ({
          role: message.role,
          content: message.content,
          timestampUtc: message.timestamp,
        })),
      })

      setMessages((currentMessages) => [
        ...currentMessages,
        createMessage('assistant', response.assistantMessage),
      ])
      setCitations(response.citations)
      setStatus({
        tone: response.isPlaceholder ? 'warning' : 'success',
        message: `Chat response received with status '${response.status}'.`,
      })
    } catch (error) {
      setMessages((currentMessages) => [
        ...currentMessages,
        createMessage('assistant', 'The chat request failed. Start the backend and try again.'),
      ])
      setStatus({
        tone: 'error',
        message: error instanceof Error ? error.message : 'Chat request failed.',
      })
    } finally {
      setIsSending(false)
    }
  }

  return (
    <main className="flex h-screen w-full bg-background md:flex-row flex-col overflow-hidden text-sm">
      <section className="flex flex-col flex-1 min-w-0 border-r bg-background">
        <header className="px-6 py-4 border-b flex-none bg-card">
          <h1 className="text-xl font-bold">Grocery Store SOP Assistant</h1>
          <p className="text-muted-foreground mt-1 text-xs">
            Powered by RAG Contexts and GPT-4o Agent.
          </p>
        </header>
        
        <StatusBanner status={status} />
        
        <div className="flex-1 overflow-y-auto">
           <ChatTranscript messages={messages} />
        </div>
        
        <div className="flex-none p-4 pb-6 bg-background">
          <ChatComposer value={draft} onChange={setDraft} onSubmit={handleSend} isBusy={isSending} />
        </div>
      </section>

      <aside className="w-full md:w-[380px] flex-none flex flex-col bg-muted/10 overflow-y-auto border-l">
        <div className="p-4 space-y-6">
          <IngestPanel
            sourcePath={sourcePath}
            onSourcePathChange={setSourcePath}
            onIngest={handleIngest}
            isBusy={isIngesting}
          />
          <CitationsPanel citations={citations} />
        </div>
      </aside>
    </main>
  )
}