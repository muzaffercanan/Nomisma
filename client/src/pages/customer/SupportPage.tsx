import { HelpCircle, Mail, Phone, MessageSquare } from 'lucide-react'

export default function CustomerSupportPage() {
  return (
    <div className="max-w-2xl mx-auto px-6 py-8 space-y-6">
      <div>
        <h1 className="page-title">Destek</h1>
        <p className="text-sm text-gray-500 mt-0.5">Size nasıl yardımcı olabiliriz?</p>
      </div>

      <div className="grid gap-4">
        <a
          href="tel:+908501234567"
          className="card flex items-center gap-4 hover:shadow-card-lg transition-shadow no-underline"
        >
          <div className="w-12 h-12 rounded-xl bg-brand-teal/10 flex items-center justify-center flex-shrink-0">
            <Phone size={22} className="text-brand-teal" />
          </div>
          <div>
            <p className="font-semibold text-gray-900">Telefon Desteği</p>
            <p className="text-sm text-gray-500">0850 123 45 67 · Hafta içi 09:00–18:00</p>
          </div>
        </a>

        <a
          href="mailto:destek@nomisma.local"
          className="card flex items-center gap-4 hover:shadow-card-lg transition-shadow no-underline"
        >
          <div className="w-12 h-12 rounded-xl bg-blue-50 flex items-center justify-center flex-shrink-0">
            <Mail size={22} className="text-blue-600" />
          </div>
          <div>
            <p className="font-semibold text-gray-900">E-posta Desteği</p>
            <p className="text-sm text-gray-500">destek@nomisma.local · 24 saat içinde yanıt</p>
          </div>
        </a>

        <div className="card flex items-center gap-4">
          <div className="w-12 h-12 rounded-xl bg-purple-50 flex items-center justify-center flex-shrink-0">
            <MessageSquare size={22} className="text-purple-600" />
          </div>
          <div>
            <p className="font-semibold text-gray-900">Canlı Destek</p>
            <p className="text-sm text-gray-500">Hafta içi 09:00–18:00 arası aktif</p>
          </div>
        </div>
      </div>

      <div className="card space-y-3">
        <div className="flex items-center gap-2">
          <HelpCircle size={18} className="text-brand-teal" />
          <h2 className="section-title">Sık Sorulan Sorular</h2>
        </div>
        {[
          { q: 'Taksit ödemem gecikirse ne olur?', a: 'Gecikmiş taksitler için gecikme faizi uygulanabilir. En kısa sürede ödemenizi yapmanızı öneririz.' },
          { q: 'Kredi başvurusu nasıl yapabilirim?', a: 'Kredi başvurusu için lütfen şubelerimizi ziyaret edin veya telefon desteğimizi arayın.' },
          { q: 'Kart bilgilerimi güncelleyebilir miyim?', a: 'Her ödeme işleminde yeni kart bilgilerini girebilirsiniz.' },
        ].map((item, i) => (
          <div key={i} className="border-t border-gray-100 pt-3 first:border-t-0 first:pt-0">
            <p className="text-sm font-medium text-gray-900">{item.q}</p>
            <p className="text-sm text-gray-500 mt-1">{item.a}</p>
          </div>
        ))}
      </div>
    </div>
  )
}
