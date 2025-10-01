
import React from 'react'
import toast from 'react-hot-toast'
import { createVehicle, deleteVehicle, getVehicles, updateVehicle } from '../api'
import VehicleForm from './VehicleForm'

type Vehicle = { id: string; plate: string; model?: string; alias?: string }

export default function VehiclesTab() {
  const [vehicles, setVehicles] = React.useState<Vehicle[]>([])
  const [loading, setLoading] = React.useState(true)
  const [editing, setEditing] = React.useState<Vehicle | null>(null)
  const [creating, setCreating] = React.useState(false)

  async function load() {
    setLoading(true)
    try {
      const data = await getVehicles()
      setVehicles(data)
    } finally {
      setLoading(false)
    }
  }

  React.useEffect(() => { load() }, [])

  return (
    <div>
      <div className="toolbar">
        <button className="btn primary" onClick={() => { setCreating(!creating); setEditing(null) }}>
          {creating ? 'Close' : 'New Vehicle'}
        </button>
        <span className="badge">{vehicles.length} items</span>
      </div>

      {creating && (
        <div style={{marginBottom:12}}>
          <VehicleForm onSubmit={async (payload) => {
            await createVehicle(payload)
            toast.success('Vehicle created')
            setCreating(false)
            await load()
          }} />
          <hr className="sep" />
        </div>
      )}

      {editing && (
        <div style={{marginBottom:12}}>
          <VehicleForm initial={editing} onCancel={() => setEditing(null)} onSubmit={async (payload) => {
            await updateVehicle(editing.id, payload)
            toast.success('Vehicle updated')
            setEditing(null)
            await load()
          }} />
          <hr className="sep" />
        </div>
      )}

      <div className="map-wrap" style={{height:'auto', padding:0}}>
        <table className="table">
          <thead>
            <tr>
              <th style={{width:220}}>Plate</th>
              <th>Alias</th>
              <th>Model</th>
              <th style={{width:220}}>Actions</th>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr><td colSpan={4}>Loading...</td></tr>
            ) : vehicles.length === 0 ? (
              <tr><td colSpan={4}>No data</td></tr>
            ) : vehicles.map(v => (
              <tr key={v.id}>
                <td>{v.plate}</td>
                <td>{v.alias ?? '-'}</td>
                <td>{v.model ?? '-'}</td>
                <td className="actions">
                  <button className="btn" onClick={() => setEditing(v)}>Edit</button>
                  <button className="btn danger" onClick={async () => {
                    if (!confirm(`Delete ${v.plate}?`)) return
                    await deleteVehicle(v.id)
                    toast.success('Vehicle deleted')
                    await load()
                  }}>Delete</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
