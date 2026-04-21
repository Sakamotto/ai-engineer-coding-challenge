import type { ChatMessage } from '../pages/ChatPage'
import { Bot, User } from 'lucide-react'

interface ChatTranscriptProps {
  messages: ChatMessage[]
}

export function ChatTranscript({ messages }: ChatTranscriptProps) {
  if (messages.length === 0) {
    return (
      <div className="h-full flex flex-col items-center justify-center p-8 text-center text-muted-foreground">
        <Bot className="w-12 h-12 mb-4 opacity-50" />
        <h2 className="text-lg font-semibold text-foreground mb-1">No messages yet</h2>
        <p className="max-w-sm">Type a message below to start chatting with the Assistant.</p>
      </div>
    )
  }

  return (
    <div className="flex-1 p-6 space-y-6">
      {messages.map((message) => (
        <div key={message.id} className={`flex gap-4 ${message.role === 'user' ? 'justify-end' : 'justify-start'}`}>
          {message.role === 'assistant' && (
            <div className="flex-shrink-0 w-8 h-8 mt-1 rounded-md bg-secondary flex items-center justify-center border">
              <Bot className="w-5 h-5 text-secondary-foreground" />
            </div>
          )}
          
          <div className={`rounded-xl px-4 py-3 max-w-[85%] ${
            message.role === 'user' 
              ? 'bg-primary text-primary-foreground rounded-tr-sm' 
              : 'bg-card border shadow-sm text-card-foreground rounded-tl-sm'
          }`}>
            <div className="whitespace-pre-wrap leading-relaxed">
              {message.content}
            </div>
            <div className={`text-[10px] mt-2 opacity-50 ${message.role === 'user' ? 'text-right' : 'text-left'}`}>
              {new Date(message.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
            </div>
          </div>

          {message.role === 'user' && (
            <div className="flex-shrink-0 w-8 h-8 mt-1 rounded-md bg-primary flex items-center justify-center">
              <User className="w-5 h-5 text-primary-foreground" />
            </div>
          )}
        </div>
      ))}
    </div>
  )
}