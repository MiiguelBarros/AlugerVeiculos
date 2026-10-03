const currencyFormatter = new Intl.NumberFormat('pt-PT', { style: 'currency', currency: 'EUR' })

export function formatCurrency(value: number): string {
  return currencyFormatter.format(value)
}
