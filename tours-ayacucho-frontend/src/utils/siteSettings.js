import apiClient from '../api/apiClient'

export const SITE_SETTINGS_KEY = 'tours_ayacucho_site_settings'

export const defaultSiteSettings = {
  companyName: 'TOURS',
  companySubtitle: 'AYACUCHO PERU',
  logoUrl: '',
  heroBadge: 'La Joya de los Andes Peruanos',
  heroTitle: 'Descubre la Magia de Ayacucho Peru',
  heroSubtitle: 'Sumergete en la riqueza cultural, historica y natural de Huamanga. Tours exclusivos, experiencias unicas e inolvidables.',
  heroStatsTours: '50+',
  heroStatsTravelers: '1K+',
  heroStatsRating: '4.9',
  heroImages: [
    {
      title: 'Aguas Turquesas de Millpu',
      imageUrl: 'https://images.unsplash.com/photo-1500530855697-b586d89ba3ee?auto=format&fit=crop&w=1400&q=80',
    },
    {
      title: 'Pampa de Ayacucho',
      imageUrl: 'https://images.unsplash.com/photo-1587595431973-160d0d94add1?auto=format&fit=crop&w=1400&q=80',
    },
    {
      title: 'Vilcashuaman Inca',
      imageUrl: 'https://images.unsplash.com/photo-1526392060635-9d6019884377?auto=format&fit=crop&w=1400&q=80',
    },
    {
      title: 'Huamanga Colonial',
      imageUrl: 'https://images.unsplash.com/photo-1533105079780-92b9be482077?auto=format&fit=crop&w=1400&q=80',
    },
  ],
}

export const getSiteSettings = () => {
  try {
    const stored = localStorage.getItem(SITE_SETTINGS_KEY)
    if (!stored) return defaultSiteSettings

    const parsed = JSON.parse(stored)
    return {
      ...defaultSiteSettings,
      ...parsed,
      heroImages: Array.from({ length: 4 }, (_, index) => ({
        ...defaultSiteSettings.heroImages[index],
        ...(parsed.heroImages?.[index] ?? {}),
      })),
    }
  } catch {
    return defaultSiteSettings
  }
}

export const fetchSiteSettings = async () => {
  const response = await apiClient.get('/site-settings')
  localStorage.setItem(SITE_SETTINGS_KEY, JSON.stringify(response.data))
  return response.data
}

export const updateSiteSettings = async (settings) => {
  const response = await apiClient.put('/admin/site-settings', settings)
  localStorage.setItem(SITE_SETTINGS_KEY, JSON.stringify(response.data))
  window.dispatchEvent(new Event('site-settings-updated'))
  return response.data
}

export const saveSiteSettings = (settings) => {
  localStorage.setItem(SITE_SETTINGS_KEY, JSON.stringify(settings))
  window.dispatchEvent(new Event('site-settings-updated'))
}
