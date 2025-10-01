import React from 'react'
import toast from 'react-hot-toast'
import { getTrackingPoints, getVehicles, uploadTrackingJson } from '../api'
import MapView from './MapView'

type Vehicle = { id: string, plate: string }
type TrackDto = { latitude: number, longitude: number, timestampUtc?: string, speedKmh?: number }

function normalizeTracking(input: any) {
  const raw =
    Array.isArray(input) ? input :
    Array.isArray(input?.items) ? input.items :
    Array.isArray(input?.data) ? input.data :
    Array.isArray(input?.points) ? input.points :
    (input ? [input] : [])

  return raw
    .filter((x: { latitude: null; longitude: null }) => x && x.latitude != null && x.longitude != null)
    .map((x: { latitude: any; longitude: any; timestampUtc: any; timestampUTC: any; dateUTC: any; date: any; speedKmh: any; speed: any }) => ({
      latitude: Number(x.latitude),
      longitude: Number(x.longitude),
      timestampUtc: x.timestampUtc ?? x.timestampUTC ?? x.dateUTC ?? x.date ?? undefined,
      speedKmh: x.speedKmh ?? x.speed ?? undefined,
    }))
}


export default function TrackingTab() {
  const [vehicles, setVehicles] = React.useState<Vehicle[]>([])
  const [plate, setPlate] = React.useState('')
  const [points, setPoints] = React.useState<TrackDto[]>([])
  const [loading, setLoading] = React.useState(false)
  const [uploading, setUploading] = React.useState(false)

  React.useEffect(() => {
    (async () => {
      try {
        const data = await getVehicles()
        setVehicles(data.map((v: any) => ({ id: v.id, plate: v.plate })))
      } catch {}
    })()
  }, [])

  async function search() {
    const found = vehicles.find(v => v.plate.toLowerCase() === plate.toLowerCase())
    if (!found) {
      toast.error('Vehicle not found for plate ' + plate)
      setPoints([])
      return
    }
    setLoading(true)
    try {
      const data = await getTrackingPoints(found.id)
      const norm = normalizeTracking(data)
      setPoints(norm)
      if (!norm.length) toast('No tracking points', { icon: 'ℹ️' })
    } finally {
      setLoading(false)
    }
  }

  async function handleFile(file: File) {
    try {
      setUploading(true)
      const text = await file.text()
      const json = JSON.parse(text)
      if (!Array.isArray(json)) throw new Error('JSON must be an array of items')
      const res = await uploadTrackingJson(json)
      // res may be {results:{Sucess:number, Errors:[]}, count:number} or problem details (207)
      if (res?.results?.Sucess != null) {
        toast.success(`Uploaded ${res.results.Sucess}/${res.count} items`)
      } else if (res?.detail || res?.title) {
        toast(res.title ?? 'Upload done', { icon: 'ℹ️' })
      } else {
        toast.success('Upload finished')
      }
    } catch (e:any) {
      // axios interceptor already shows details; but ensure feedback for parse errors
      if (e?.message?.includes('JSON')) toast.error(e.message)
    } finally {
      setUploading(false)
    }
  }

  const mapPoints = points.map(p => ({ lat: p.latitude, lng: p.longitude, ts: p.timestampUtc, speed: p.speedKmh }))

  return (
    <div>
      <div className="toolbar" style={{justifyContent:'space-between', flexWrap:'wrap'}}>
        <div className="flex">
          <input className="input" placeholder="Search by plate (e.g. ABC1D23)"
                value={plate} onChange={e => setPlate(e.target.value)} style={{maxWidth:320}} />
          <button className="btn primary" onClick={search} disabled={!plate}>Search</button>
          <span className="badge">Known vehicles: {vehicles.length}</span>
        </div>

        <div className="flex">
          <input id="jsonFile" type="file" accept="application/json,.json"
                 onChange={e => { const f = e.target.files?.[0]; if (f) handleFile(f); }}
                 style={{display:'none'}} />
          <label htmlFor="jsonFile" className="btn">{uploading ? 'Uploading...' : 'Upload JSON'}</label>
        </div>
      </div>

      {loading ? <div>Loading...</div> : <MapView points={mapPoints} />}

      <p style={{marginTop:8, color:'#9ca3af', fontSize:12}}>
        Upload expects an array of tracking items (same shape as <code>TrackingUploadItemDto</code>).
      </p>
    </div>
  )
}
