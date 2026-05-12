type Props = { name: string }

export function GreetingHero({ name }: Props) {
  return (
    <div className="relative overflow-hidden rounded-2xl bg-gradient-to-br from-brand-teal to-brand-navy p-8 text-white">
      {/* Decorative circles */}
      <div className="absolute -top-6 -right-6 w-32 h-32 rounded-full bg-white/10" />
      <div className="absolute top-8 right-16 w-16 h-16 rounded-full bg-white/10" />
      <div className="absolute -bottom-4 right-32 w-24 h-24 rounded-full bg-white/5" />

      <div className="relative z-10">
        <p className="text-white/70 text-sm font-medium mb-1">Hoş geldiniz</p>
        <h1 className="text-3xl font-bold">Merhaba,</h1>
        <h1 className="text-3xl font-bold">{name}!</h1>
        <p className="text-white/60 text-sm mt-3">Finansal durumunuza genel bir bakış</p>
      </div>
    </div>
  )
}
