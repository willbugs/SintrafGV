/** Fuso oficial Brasil (São Paulo / Brasília). Armazenamento: UTC; exibição e formulários: horário local BR. */
export const BRASILIA_TIMEZONE = 'America/Sao_Paulo'
export const BRASILIA_UTC_OFFSET = '-03:00'

export function parseUtcIso(iso: string | undefined | null): Date | null {
  if (!iso?.trim()) return null
  const s = iso.trim()
  const normalized = /[zZ]$|[+-]\d{2}:\d{2}$/.test(s) ? s : `${s}Z`
  const d = new Date(normalized)
  return Number.isNaN(d.getTime()) ? null : d
}

/** Converte ISO UTC da API para valor do input datetime-local (horário de Brasília). */
export function utcIsoToDatetimeLocalBr(iso: string): string {
  const d = parseUtcIso(iso)
  if (!d) return ''
  const parts = new Intl.DateTimeFormat('en-GB', {
    timeZone: BRASILIA_TIMEZONE,
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    hourCycle: 'h23',
  }).formatToParts(d)
  const v = (t: string) => parts.find((p) => p.type === t)?.value ?? ''
  return `${v('year')}-${v('month')}-${v('day')}T${v('hour')}:${v('minute')}`
}

/** Converte datetime-local (horário de Brasília) para ISO UTC enviado à API. */
export function brDatetimeLocalToUtcIso(local: string): string {
  if (!local?.trim()) return ''
  const match = /^(\d{4}-\d{2}-\d{2})T(\d{2}:\d{2})(?::\d{2})?$/.exec(local.trim())
  if (!match) return new Date(local).toISOString()
  return new Date(`${match[1]}T${match[2]}:00${BRASILIA_UTC_OFFSET}`).toISOString()
}

export function formatDateTimeBr(iso: string | undefined | null): string {
  const d = parseUtcIso(iso ?? '')
  if (!d) return '—'
  return d.toLocaleString('pt-BR', {
    timeZone: BRASILIA_TIMEZONE,
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

export function formatDateBr(iso: string | undefined | null): string {
  const d = parseUtcIso(iso ?? '')
  if (!d) return '—'
  return d.toLocaleDateString('pt-BR', {
    timeZone: BRASILIA_TIMEZONE,
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  })
}
