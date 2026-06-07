<template>
  <div class="h-[calc(100vh-3.5rem)] overflow-y-auto">
    <div class="max-w-6xl mx-auto p-6 space-y-6">

      <!-- Page header：小螢幕垂直堆疊，大螢幕水平排列 -->
      <div class="flex flex-col lg:flex-row lg:items-center gap-3">
        <div class="flex-1 min-w-0">
          <h1 class="text-2xl font-bold text-white">營運報表</h1>
          <p class="text-sm text-gray-500 mt-0.5">查詢結算傳票統計與明細</p>
        </div>

        <!-- 篩選控制列：允許換行 -->
        <div class="flex flex-wrap items-center gap-2">
          <!-- 日期區間 -->
          <div class="flex items-center gap-2 glass px-3 py-2 flex-shrink-0">
            <svg class="w-4 h-4 text-gray-400 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/>
            </svg>
            <input
              id="date-from"
              v-model="dateFrom"
              type="date"
              class="bg-transparent text-sm text-gray-200 focus:outline-none w-32"
            />
            <span class="text-gray-600">—</span>
            <input
              id="date-to"
              v-model="dateTo"
              type="date"
              class="bg-transparent text-sm text-gray-200 focus:outline-none w-32"
            />
          </div>

          <!-- 快捷範圍 -->
          <div class="flex items-center gap-1">
            <button class="btn-ghost btn-sm" @click="setRange('today')">今日</button>
            <button class="btn-ghost btn-sm" @click="setRange('week')">本週</button>
            <button class="btn-ghost btn-sm" @click="setRange('month')">本月</button>
          </div>

          <!-- 例外單篩選 -->
          <label class="flex items-center gap-1.5 cursor-pointer select-none">
            <input id="exception-filter" v-model="filterException" type="checkbox"
              class="w-3.5 h-3.5 rounded accent-red-500" />
            <span class="text-xs text-gray-400 whitespace-nowrap">僅例外單</span>
          </label>

          <!-- 查詢按鈕 -->
          <button id="search-btn" class="btn-primary btn-sm" :disabled="loading" @click="loadData">
            <svg v-if="loading" class="w-3.5 h-3.5 animate-spin" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/>
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"/>
            </svg>
            <svg v-else class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/>
            </svg>
            查詢
          </button>
        </div>
      </div>

      <!-- Summary cards -->
      <div class="grid grid-cols-2 xl:grid-cols-4 gap-4">
        <SummaryCard
          label="結算車次"
          :value="summary.totalVehicles"
          unit="台"
          icon="truck"
          color="blue"
          :loading="loading"
        />
        <SummaryCard
          label="總淨重"
          :value="summary.totalNetWeightKg"
          unit="kg"
          icon="scale"
          color="green"
          :loading="loading"
          :format="v => v.toLocaleString()"
        />
        <SummaryCard
          label="結算總金額"
          :value="summary.totalAmount"
          unit="元"
          icon="cash"
          color="amber"
          :loading="loading"
          :format="v => v.toLocaleString()"
          prefix="$"
        />
        <SummaryCard
          label="例外件數"
          :value="summary.exceptionCount"
          unit="件"
          icon="warning"
          color="red"
          :loading="loading"
        />
      </div>

      <!-- Tickets table -->
      <div class="glass overflow-hidden">
        <!-- Table header -->
        <div class="flex items-center justify-between px-4 py-3 border-b border-white/10">
          <h2 class="text-sm font-semibold text-gray-300">傳票明細</h2>
          <span class="text-xs text-gray-600">共 {{ tickets.length }} 筆</span>
        </div>

        <!-- Loading skeleton -->
        <div v-if="loading" class="p-6 space-y-3">
          <div v-for="i in 5" :key="i" class="h-10 bg-white/5 rounded-lg animate-pulse"></div>
        </div>

        <!-- Empty state -->
        <div v-else-if="tickets.length === 0" class="py-16 text-center text-gray-600">
          <svg class="w-12 h-12 mx-auto mb-3 opacity-30" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1"
              d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"/>
          </svg>
          <p class="text-sm">此區間無已結算傳票</p>
        </div>

        <!-- Table -->
        <div v-else class="overflow-x-auto">
          <table class="w-full text-sm">
            <thead>
              <tr class="border-b border-white/10 text-left">
                <th class="px-4 py-2.5 text-xs font-medium text-gray-500">傳票號</th>
                <th class="px-4 py-2.5 text-xs font-medium text-gray-500">車號</th>
                <th class="px-4 py-2.5 text-xs font-medium text-gray-500">農民/村別</th>
                <th class="px-4 py-2.5 text-xs font-medium text-gray-500">米種</th>
                <th class="px-4 py-2.5 text-xs font-medium text-gray-500 text-right">總重(kg)</th>
                <th class="px-4 py-2.5 text-xs font-medium text-gray-500 text-right">台斤</th>
                <th class="px-4 py-2.5 text-xs font-medium text-gray-500 text-right">單價</th>
                <th class="px-4 py-2.5 text-xs font-medium text-gray-500 text-right">金額</th>
                <th class="px-4 py-2.5 text-xs font-medium text-gray-500 text-center">例外</th>
                <th class="px-4 py-2.5 text-xs font-medium text-gray-500">結算時間</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="t in tickets"
                :key="t.id"
                class="border-b border-white/5 hover:bg-white/3 transition-colors"
                :class="{ 'bg-red-500/5': t.isException }"
              >
                <td class="px-4 py-2.5 font-mono text-xs text-gray-400">{{ t.ticketNo }}</td>
                <td class="px-4 py-2.5 font-mono font-medium text-white">{{ t.vehicleNo }}</td>
                <td class="px-4 py-2.5">
                  <p class="text-gray-200">{{ t.farmerName || '—' }}</p>
                  <p class="text-xs text-gray-600">{{ t.village || '' }}</p>
                </td>
                <td class="px-4 py-2.5 text-gray-300 text-xs">{{ t.riceTypeName }}</td>
                <td class="px-4 py-2.5 font-mono text-right text-gray-300">{{ t.grossWeightKg.toLocaleString() }}</td>
                <td class="px-4 py-2.5 font-mono text-right text-brand-300 font-medium">{{ t.netWeightJin.toLocaleString() }}</td>
                <td class="px-4 py-2.5 font-mono text-right text-gray-400 text-xs">${{ t.priceSnapshot }}</td>
                <td class="px-4 py-2.5 font-mono text-right font-semibold text-green-400">${{ t.totalAmount.toLocaleString() }}</td>
                <td class="px-4 py-2.5 text-center">
                  <span v-if="t.isException" class="inline-block">
                    <span class="badge badge-unloading text-[10px]" :title="t.exceptionReason">
                      {{ t.exceptionReason || '例外' }}
                    </span>
                  </span>
                  <span v-else class="text-gray-700">—</span>
                </td>
                <td class="px-4 py-2.5 text-xs text-gray-500 whitespace-nowrap">{{ fmtDateTime(t.settledAt) }}</td>
              </tr>
            </tbody>

            <!-- Footer totals -->
            <tfoot>
              <tr class="bg-white/5 border-t border-white/10">
                <td colspan="4" class="px-4 py-2.5 text-xs font-semibold text-gray-400">合計</td>
                <td class="px-4 py-2.5 font-mono text-right text-xs text-gray-300">
                  {{ tickets.reduce((s, t) => s + t.grossWeightKg, 0).toLocaleString() }}
                </td>
                <td class="px-4 py-2.5 font-mono text-right text-sm font-bold text-brand-300">
                  {{ tickets.reduce((s, t) => s + t.netWeightJin, 0).toLocaleString() }}
                </td>
                <td class="px-4 py-2.5"></td>
                <td class="px-4 py-2.5 font-mono text-right text-sm font-bold text-green-400">
                  ${{ tickets.reduce((s, t) => s + t.totalAmount, 0).toLocaleString() }}
                </td>
                <td colspan="2"></td>
              </tr>
            </tfoot>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, h, defineComponent } from 'vue'
import api from '@/api'

// ─── SummaryCard inline component ────────────────────────────
const SummaryCard = defineComponent({
  props: {
    label:   String, value: [Number], unit: String,
    icon:    String, color: String, loading: Boolean,
    format:  { type: Function, default: v => String(v) },
    prefix:  { type: String, default: '' },
  },
  setup(p) {
    const colorMap = {
      blue:  { bg: 'bg-blue-500/10',  border: 'border-blue-500/20',  text: 'text-blue-300',  icon: 'text-blue-400' },
      green: { bg: 'bg-green-500/10', border: 'border-green-500/20', text: 'text-green-300', icon: 'text-green-400' },
      amber: { bg: 'bg-amber-500/10', border: 'border-amber-500/20', text: 'text-amber-300', icon: 'text-amber-400' },
      red:   { bg: 'bg-red-500/10',   border: 'border-red-500/20',   text: 'text-red-300',   icon: 'text-red-400' },
    }
    const iconPaths = {
      truck: 'M9 17a2 2 0 11-4 0 2 2 0 014 0zM19 17a2 2 0 11-4 0 2 2 0 014 0zM13 16V6a1 1 0 00-1-1H4a1 1 0 00-1 1v10l2 2h1m6-1h1l6-6v-3l-4-2h-3v5l-2 2',
      scale: 'M3 6l3 1m0 0l-3 9a5.002 5.002 0 006.001 0M6 7l3 9M6 7l6-2m6 2l3-1m-3 1l-3 9a5.002 5.002 0 006.001 0M18 7l3 9m-3-9l-6-2m0-2v2m0 16V5m0 16H9m3 0h3',
      cash: 'M17 9V7a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2m2 4h10a2 2 0 002-2v-6a2 2 0 00-2-2H9a2 2 0 00-2 2v6a2 2 0 002 2zm7-5a2 2 0 11-4 0 2 2 0 014 0z',
      warning: 'M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z',
    }
    return () => {
      const c = colorMap[p.color] || colorMap.blue
      return h('div', { class: `glass p-4 border ${c.border} ${c.bg}` }, [
        h('div', { class: 'flex items-start justify-between mb-3' }, [
          h('span', { class: 'text-xs text-gray-400 font-medium' }, p.label),
          h('svg', { class: `w-5 h-5 ${c.icon}`, fill: 'none', viewBox: '0 0 24 24', stroke: 'currentColor', 'stroke-width': '1.5' }, [
            h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: iconPaths[p.icon] || '' })
          ]),
        ]),
        p.loading
          ? h('div', { class: 'h-8 bg-white/10 rounded animate-pulse' })
          : h('p', { class: `text-2xl font-bold font-mono ${c.text}` },
              `${p.prefix}${p.format(p.value ?? 0)}`),
        h('p', { class: 'text-xs text-gray-600 mt-0.5' }, p.unit),
      ])
    }
  }
})

// ─── State ────────────────────────────────────────────────────
const today = new Date().toISOString().slice(0, 10)
const dateFrom = ref(today)
const dateTo   = ref(today)
const filterException = ref(false)
const loading  = ref(false)
const tickets  = ref([])
const summary  = ref({ totalVehicles: 0, totalNetWeightKg: 0, totalAmount: 0, exceptionCount: 0 })

// ─── Quick range setters ──────────────────────────────────────
function setRange(range) {
  const now = new Date()
  if (range === 'today') {
    dateFrom.value = dateTo.value = now.toISOString().slice(0, 10)
  } else if (range === 'week') {
    const mon = new Date(now)
    mon.setDate(now.getDate() - now.getDay() + 1)
    dateFrom.value = mon.toISOString().slice(0, 10)
    dateTo.value   = now.toISOString().slice(0, 10)
  } else if (range === 'month') {
    dateFrom.value = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-01`
    dateTo.value   = now.toISOString().slice(0, 10)
  }
  loadData()
}

// ─── Load data ────────────────────────────────────────────────
async function loadData() {
  loading.value = true
  try {
    const params = {
      status: 'settled',
      dateFrom: dateFrom.value,
      dateTo:   dateTo.value,
    }
    if (filterException.value) params.isException = true

    const [ticketsRes, summaryRes] = await Promise.all([
      api.get('/tickets',            { params }),
      api.get('/dashboard/summary',  { params: { dateFrom: dateFrom.value, dateTo: dateTo.value } }),
    ])

    tickets.value = ticketsRes.data
    summary.value = summaryRes.data
  } catch (e) {
    console.error('Dashboard load failed:', e)
  } finally {
    loading.value = false
  }
}

function fmtDateTime(iso) {
  if (!iso) return '—'
  try {
    return new Date(iso).toLocaleString('zh-TW', {
      month: '2-digit', day: '2-digit',
      hour: '2-digit', minute: '2-digit', hour12: false,
    })
  } catch { return iso }
}

onMounted(() => loadData())
</script>
