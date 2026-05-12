export const API_BASE_URL = import.meta.env.VITE_API_URL ?? 'https://localhost:7176'

export type Role = 'Admin' | 'Customer'

export type Session = {
  token: string
  email: string
  customerId: string | null
  roles: Role[]
}

export type LoanType = 'Personal' | 'Education' | 'Vehicle'
export type LoanStatus = 'Active' | 'Closed'
export type InstallmentStatus = 'Unpaid' | 'Paid' | 'Overdue'
export type PaymentStatus = 'Completed' | 'Failed'
export type GatewayStatus = 'NotSent' | 'Approved' | 'Declined'

export type Customer = {
  id: string
  customerNumber: string
  firstName: string
  lastName: string
  fullName: string
  nationalId: string
  email: string
  phoneNumber: string
  address: string
  dateOfBirth: string
  createdAtUtc: string
  updatedAtUtc: string | null
}

export type CreateCustomerRequest = {
  firstName: string
  lastName: string
  nationalId: string
  email: string
  phoneNumber: string
  address: string
  dateOfBirth: string
  password: string
}

export type UpdateCustomerRequest = Omit<CreateCustomerRequest, 'nationalId' | 'password'>

export type Installment = {
  id: string
  loanId: string
  installmentNumber: number
  principalAmount: number
  profitAmount: number
  amount: number
  dueDate: string
  status: InstallmentStatus
  paidAtUtc: string | null
  hasPayment: boolean
}

export type Loan = {
  id: string
  customerId: string
  type: LoanType
  principalAmount: number
  profitRate: number
  termMonths: number
  startDate: string
  status: LoanStatus
  creditScore: number
  totalProfit: number
  totalDebt: number
  paidAmount: number
  remainingDebt: number
  createdAtUtc: string
  closedAtUtc: string | null
  installments: Installment[]
}

export type CreateLoanRequest = {
  customerId: string
  type: LoanType
  principalAmount: number
  profitRate: number
  termMonths: number
  startDate: string
}

export type Payment = {
  id: string
  installmentId: string
  loanId: string
  customerId: string
  amount: number
  paidAtUtc: string
  status: PaymentStatus
  gatewayStatus: GatewayStatus
  gatewayTransactionId: string
  failureReason: string | null
}

export type CustomerSummary = {
  customerId: string
  customerNumber: string
  fullName: string
  totalLoanDebt: number
  remainingPrincipal: number
  remainingDebt: number
  overdueInstallmentCount: number
  paidInstallments: Installment[]
  unpaidInstallments: Installment[]
}

export type CreatePaymentRequest = {
  installmentId: string
  amount: number
  cardHolderName: string
  cardNumber: string
  cvv: string
  expiryMonth: number
  expiryYear: number
}

type RequestOptions = {
  token?: string
  method?: string
  body?: unknown
}

export async function apiRequest<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: options.method ?? 'GET',
    headers: {
      'Content-Type': 'application/json',
      ...(options.token ? { Authorization: `Bearer ${options.token}` } : {}),
    },
    body: options.body === undefined ? undefined : JSON.stringify(options.body),
  })

  if (!response.ok) {
    let message = `HTTP ${response.status}`
    try {
      const payload = await response.json()
      message = payload.message ?? message
    } catch {
      // Empty error body.
    }

    throw new Error(message)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return response.json() as Promise<T>
}

export const api = {
  login: (email: string, password: string) =>
    apiRequest<Session>('/api/auth/login', {
      method: 'POST',
      body: { email, password },
    }),
  customers: (token: string) => apiRequest<Customer[]>('/api/customers', { token }),
  customer: (token: string, id: string) => apiRequest<Customer>(`/api/customers/${id}`, { token }),
  createCustomer: (token: string, body: CreateCustomerRequest) =>
    apiRequest<Customer>('/api/customers', { token, method: 'POST', body }),
  updateCustomer: (token: string, id: string, body: UpdateCustomerRequest) =>
    apiRequest<Customer>(`/api/customers/${id}`, { token, method: 'PUT', body }),
  deleteCustomer: (token: string, id: string) =>
    apiRequest<void>(`/api/customers/${id}`, { token, method: 'DELETE' }),
  customerSummary: (token: string, id: string) =>
    apiRequest<CustomerSummary>(`/api/customers/${id}/summary`, { token }),
  mySummary: (token: string) => apiRequest<CustomerSummary>('/api/customers/me/summary', { token }),
  loans: (token: string, customerId?: string) =>
    apiRequest<Loan[]>(`/api/loans${customerId ? `?customerId=${customerId}` : ''}`, { token }),
  loan: (token: string, id: string) => apiRequest<Loan>(`/api/loans/${id}`, { token }),
  createLoan: (token: string, body: CreateLoanRequest) =>
    apiRequest<Loan>('/api/loans', { token, method: 'POST', body }),
  closeLoan: (token: string, id: string) =>
    apiRequest<Loan>(`/api/loans/${id}`, { token, method: 'PUT', body: { status: 'Closed' } }),
  installments: (token: string, loanId: string) =>
    apiRequest<Installment[]>(`/api/loans/${loanId}/installments`, { token }),
  payments: (token: string) => apiRequest<Payment[]>('/api/payments', { token }),
  createPayment: (token: string, body: CreatePaymentRequest) =>
    apiRequest<Payment>('/api/payments', { token, method: 'POST', body }),
}

export function formatMoney(value: number) {
  return new Intl.NumberFormat('tr-TR', {
    style: 'currency',
    currency: 'TRY',
    maximumFractionDigits: 2,
  }).format(value)
}

export function formatDate(value: string | null) {
  if (!value) return '-'
  return new Intl.DateTimeFormat('tr-TR').format(new Date(value))
}

export function loanTypeLabel(type: LoanType) {
  const labels: Record<LoanType, string> = {
    Personal: 'Ihtiyac',
    Education: 'Egitim',
    Vehicle: 'Tasit',
  }
  return labels[type]
}

export function installmentStatusLabel(status: InstallmentStatus) {
  const labels: Record<InstallmentStatus, string> = {
    Unpaid: 'Odenmedi',
    Paid: 'Odendi',
    Overdue: 'Gecikmis',
  }
  return labels[status]
}

