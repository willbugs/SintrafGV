/** Fuso oficial Brasil (São Paulo / Brasília). Armazenamento: UTC; exibição: horário local BR. */
export const BRASILIA_TIMEZONE = 'America/Sao_Paulo'

export function parseUtcIso(iso: string | undefined | null): Date | null {
  if (!iso?.trim()) return null
  const s = iso.trim()
  const normalized = /[zZ]$|[+-]\d{2}:\d{2}$/.test(s) ? s : `${s}Z`
  const d = new Date(normalized)
  return Number.isNaN(d.getTime()) ? null : d
}

const dateTimeFormatOptions: Intl.DateTimeFormatOptions = {
  timeZone: BRASILIA_TIMEZONE,
  day: '2-digit',
  month: '2-digit',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
}

const dateTimeSecondsFormatOptions: Intl.DateTimeFormatOptions = {
  ...dateTimeFormatOptions,
  second: '2-digit',
}

export function formatDateTimeBr(iso: string | undefined | null): string {
  const d = parseUtcIso(iso ?? '')
  if (!d) return '—'
  return d.toLocaleString('pt-BR', dateTimeFormatOptions)
}

export function formatDateTimeSecondsBr(iso: string | undefined | null): string {
  const d = parseUtcIso(iso ?? '')
  if (!d) return '—'
  return d.toLocaleString('pt-BR', dateTimeSecondsFormatOptions)
}

export function isWithinVotingPeriodBr(inicioIso: string, fimIso: string, nowMs: number = Date.now()): boolean {
  const inicio = parseUtcIso(inicioIso)
  const fim = parseUtcIso(fimIso)
  if (!inicio || !fim) return false
  return nowMs >= inicio.getTime() && nowMs <= fim.getTime()
}
