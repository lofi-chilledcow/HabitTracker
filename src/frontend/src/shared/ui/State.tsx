export function LoadingState({ label = 'Loading' }: { label?: string }) {
  return <div className="state">{label}</div>
}

export function EmptyState({ title, detail }: { title: string; detail?: string }) {
  return (
    <div className="state">
      <strong>{title}</strong>
      {detail ? <span>{detail}</span> : null}
    </div>
  )
}

export function ErrorState({ message }: { message: string }) {
  return <div className="state error">{message}</div>
}
