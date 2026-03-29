This is the Next.js frontend for MzansiLegal. It uses the App Router, `next-intl`, Ant Design, and an organic shell design system tuned for demo storytelling.

## Getting Started

First, install dependencies and run the development server:

```bash
npm install
npm run dev
```

Open [http://localhost:3000](http://localhost:3000) with your browser to see the result.

## Canonical Routes

- `/en` home
- `/en/ask` canonical question-and-answer flow
- `/en/chat` legacy compatibility route that redirects into `/ask`
- `/en/contracts` contract analysis list
- `/en/contracts/maple-street-lease` contract detail demo route
- `/en/rights` rights explorer
- `/en/admin/dashboard` admin storytelling dashboard

## Validation

Run these before sign-off:

```bash
npm run lint
npm run build
```

Manual demo checks should confirm:

- locale switching preserves the current route family
- `/chat` redirects into the ask journey
- the shared shell stays usable at mobile widths
- the admin dashboard shows summary cards plus a chart insight area

## Notes

- Locale message files currently exist for `en`, `zu`, `st`, and `af`
- The dashboard uses `@ant-design/charts`
- The visual system is driven by shared CSS variables in `src/styles/globals.css`
