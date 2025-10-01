
export type VehicleDto = { id: string; plate: string; model?: string; alias?: string }
export type VehicleCreateDto = { plate: string; model?: string; alias?: string }
export type VehicleUpdateDto = { plate: string; model?: string; alias?: string }
export type TrackingPointDto = { id: string; vehicleId: string; latitude: number; longitude: number; timestampUtc: string; speedKmh?: number; source?: string }
export type ValidationProblem = { type?: string; title?: string; status?: number; detail?: string; instance?: string; errors?: Record<string, string[]> }
