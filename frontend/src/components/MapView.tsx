import React, { useEffect } from 'react'
import { MapContainer, TileLayer, Polyline, Marker, Popup, useMap } from 'react-leaflet'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'

import iconUrl from 'leaflet/dist/images/marker-icon.png'
import icon2xUrl from 'leaflet/dist/images/marker-icon-2x.png'
import shadowUrl from 'leaflet/dist/images/marker-shadow.png'
L.Icon.Default.mergeOptions({ iconUrl, iconRetinaUrl: icon2xUrl, shadowUrl })

type Point = { lat: number; lng: number; ts?: string; speed?: number }

function FitToPoints({ points }: { points: Point[] }) {
  const map = useMap()
  useEffect(() => {
    if (!points.length) return
    if (points.length === 1) {
      map.setView([points[0].lat, points[0].lng], 15)
      return
    }
    const bounds = L.latLngBounds(points.map(p => [p.lat, p.lng]) as [number, number][])
    map.fitBounds(bounds, { padding: [40, 40] })
  }, [points, map])
  return null
}

export default function MapView({ points }: { points: Point[] }) {
  // tipagem explícita para evitar o erro do TS no center/positions
  const center: [number, number] = points.length
    ? [points[0].lat, points[0].lng]
    : [-23.55, -46.63]

  const path: [number, number][] = points.map(p => [p.lat, p.lng])

  return (
    <div className="map-wrap">
      <MapContainer center={center} zoom={12} scrollWheelZoom style={{ height: '100%', width: '100%' }}>
        <TileLayer
          attribution='&copy; <a href="https://www.openstreetmap.org/">OpenStreetMap</a> contributors'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />

        <FitToPoints points={points} />

        {path.length > 1 && (
          <Polyline positions={path} pathOptions={{ color: '#22d3ee', weight: 4, opacity: 0.85 }} />
        )}

        {points.map((p, i) => (
          <Marker key={i} position={[p.lat, p.lng]}>
            <Popup>
              <div>
                {p.ts && <div><b>{p.ts}</b></div>}
                {p.speed != null && <div>Speed: {p.speed} km/h</div>}
                <div>({p.lat.toFixed(5)}, {p.lng.toFixed(5)})</div>
              </div>
            </Popup>
          </Marker>
        ))}
      </MapContainer>
    </div>
  )
}
