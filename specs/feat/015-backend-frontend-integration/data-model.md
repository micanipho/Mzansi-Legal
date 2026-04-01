# Data Model: Frontend Integration

## Entities (Frontend)

### Message (UI Component state)
- `id`: `string` (UUID)
- `type`: `'user' | 'bot'`
- `text`: `string`
- `status`: `'sending' | 'sent' | 'error'`
- `citations`: `Citation[]` (optional, only for 'bot')
- `timestamp`: `Date`

### Citation
- `actName`: `string`
- `sectionNumber`: `string`
- `excerpt`: `string`
- `relevanceScore`: `number`

### ConversationContext
- `messages`: `Message[]`
- `isLoading`: `boolean`
- `error`: `string | null`

## Relationships
- A `Conversation` contains an ordered list of `Message` objects.
- A `bot` message can have multiple `Citation` objects.
- Each `Message` is mapped from/to the backend `AskQuestionRequest` and `RagAnswerResult`.
