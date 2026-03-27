import { Component, type ErrorInfo, type ReactNode } from 'react'

interface Props {
  name: string
  children: ReactNode
}

interface State {
  hasError: boolean
}

export class RemoteErrorBoundary extends Component<Props, State> {
  state: State = { hasError: false }

  static getDerivedStateFromError(): State {
    return { hasError: true }
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error(`[MFE] ${this.props.name} failed to load:`, error, info)
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="flex flex-col items-center justify-center min-h-[50vh] text-gray-400">
          <p className="text-base">Failed to load <strong>{this.props.name}</strong>.</p>
          <p className="text-sm mt-1">
            Make sure the app is built and running on its port.
          </p>
        </div>
      )
    }
    return this.props.children
  }
}
