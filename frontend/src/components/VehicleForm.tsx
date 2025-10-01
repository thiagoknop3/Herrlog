
import React from 'react'
import type { VehicleCreateDto } from '../types'

type Props = {
  initial?: Partial<VehicleCreateDto>
  onSubmit: (payload: VehicleCreateDto) => Promise<void>
  submitting?: boolean
  onCancel?: () => void
}

export default function VehicleForm({ initial, onSubmit, submitting, onCancel }: Props) {
  const [plate, setPlate] = React.useState(initial?.plate ?? '')
  const [alias, setAlias] = React.useState(initial?.alias ?? '')
  const [model, setModel] = React.useState(initial?.model ?? '')

  return (
    <form onSubmit={async (e) => {
      e.preventDefault()
      await onSubmit({ plate, alias, model })
    }}>
      <div className="row">
        <div>
          <label className="label">Plate *</label>
          <input className="input" value={plate} onChange={e => setPlate(e.target.value)} required />
        </div>
        <div>
          <label className="label">Alias</label>
          <input className="input" value={alias} onChange={e => setAlias(e.target.value)} />
        </div>
        <div>
          <label className="label">Model</label>
          <input className="input" value={model} onChange={e => setModel(e.target.value)} />
        </div>
      </div>
      <div className="flex" style={{marginTop:12}}>
        <button className="btn primary" type="submit" disabled={submitting}>Save</button>
        {onCancel && <button className="btn ghost" type="button" onClick={onCancel}>Cancel</button>}
      </div>
    </form>
  )
}
