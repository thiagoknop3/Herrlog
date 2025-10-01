
import React from 'react'
import VehiclesTab from './components/VehiclesTab'
import TrackingTab from './components/TrackingTab'

export default function App() {
  const [tab, setTab] = React.useState<'vehicles'|'tracking'>('vehicles')

  return (
    <div className="app">
      <h1 style={{marginBottom: 10}}>Herrlog — Vehicles & Tracking</h1>
      <div className="tabs">
        <button className={`tab ${tab === 'vehicles' ? 'active' : ''}`} onClick={() => setTab('vehicles')}>Vehicles</button>
        <button className={`tab ${tab === 'tracking' ? 'active' : ''}`} onClick={() => setTab('tracking')}>Tracking</button>
      </div>

      <div className="card">
        {tab === 'vehicles' ? <VehiclesTab /> : <TrackingTab />}
      </div>
    </div>
  )
}
