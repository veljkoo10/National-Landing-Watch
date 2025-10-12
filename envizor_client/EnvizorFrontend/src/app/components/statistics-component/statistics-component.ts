import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../pipes/translation-pipe';

// Highcharts
import * as Highcharts from 'highcharts';
import { HighchartsChartComponent, provideHighcharts } from 'highcharts-angular';

// Angular Material (table/search/paginator)
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';

// Types
type Region = {
  id: number;
  name: string;
  population: number;
  areaKm2?: number; // optional
};

type Landfill = {
  id: number;
  regionId: number;
  lat: number;
  lng: number;
  kind: 'sanitary' | 'unsanitary' | 'wild';
  size: 'small' | 'medium' | 'large';
  yearCreated: number;
  name: string;
};

type Monitoring = {
  id: number;
  landfillId: number;
  year: number;
  volumeM3: number;
  wasteTons: number;
  ch4Tons: number;
  co2eqTons: number;
};

@Component({
  selector: 'app-statistics',
  standalone: true,
  imports: [
    CommonModule,
    HighchartsChartComponent,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    TranslatePipe
  ],
  templateUrl: './statistics-component.html',
  styleUrls: ['./statistics-component.css']
})
export class StatisticsComponent implements OnInit {

  // ===== Mock data (swap with API later) =====
  regions: Region[] = [
    { id: 1, name: 'Vojvodina', population: 1900000, areaKm2: 21506 },
    { id: 2, name: 'Belgrade', population: 1675000, areaKm2: 3226 },
    { id: 3, name: 'Western Serbia', population: 1200000, areaKm2: 26000 },
    { id: 4, name: 'Šumadija i Pomoravlje', population: 1000000, areaKm2: 13000 },
    { id: 5, name: 'Eastern Serbia', population: 900000, areaKm2: 20000 },
    { id: 6, name: 'Southern Serbia', population: 1100000, areaKm2: 18000 }
  ];

  landfills: Landfill[] = [
    { id: 101, regionId: 1, lat: 45.25, lng: 19.70, kind: 'unsanitary', size: 'large', yearCreated: 1989, name: 'Futog' },
    { id: 102, regionId: 1, lat: 45.24, lng: 19.88, kind: 'unsanitary', size: 'medium', yearCreated: 1996, name: 'Petrovaradin' },
    { id: 201, regionId: 2, lat: 44.80, lng: 20.46, kind: 'sanitary', size: 'large', yearCreated: 2005, name: 'Vinča' },
    { id: 301, regionId: 3, lat: 43.85, lng: 19.84, kind: 'wild', size: 'small', yearCreated: 2010, name: 'Tara-Rampa' },
    { id: 501, regionId: 5, lat: 44.23, lng: 22.53, kind: 'unsanitary', size: 'large', yearCreated: 1993, name: 'Bor-Jugo' }
  ];

  monitoring: Monitoring[] = [
    // landfill 101
    { id: 1, landfillId: 101, year: 2021, volumeM3: 50000, wasteTons: 150000, ch4Tons: 1200, co2eqTons: 30000 },
    { id: 2, landfillId: 101, year: 2022, volumeM3: 52000, wasteTons: 152000, ch4Tons: 1250, co2eqTons: 31000 },
    { id: 3, landfillId: 101, year: 2023, volumeM3: 54000, wasteTons: 155000, ch4Tons: 1280, co2eqTons: 32000 },

    // landfill 102
    { id: 4, landfillId: 102, year: 2021, volumeM3: 20000, wasteTons: 60000, ch4Tons: 500, co2eqTons: 13000 },
    { id: 5, landfillId: 102, year: 2022, volumeM3: 21000, wasteTons: 61000, ch4Tons: 520, co2eqTons: 13300 },
    { id: 6, landfillId: 102, year: 2023, volumeM3: 21500, wasteTons: 61500, ch4Tons: 535, co2eqTons: 13550 },

    // landfill 201
    { id: 7, landfillId: 201, year: 2021, volumeM3: 90000, wasteTons: 300000, ch4Tons: 2000, co2eqTons: 50000 },
    { id: 8, landfillId: 201, year: 2022, volumeM3: 93000, wasteTons: 305000, ch4Tons: 2050, co2eqTons: 51000 },
    { id: 9, landfillId: 201, year: 2023, volumeM3: 95000, wasteTons: 310000, ch4Tons: 2100, co2eqTons: 52000 },

    // landfill 301
    { id: 10, landfillId: 301, year: 2022, volumeM3: 8000, wasteTons: 15000, ch4Tons: 130, co2eqTons: 3200 },
    { id: 11, landfillId: 301, year: 2023, volumeM3: 8200, wasteTons: 15500, ch4Tons: 140, co2eqTons: 3300 },

    // landfill 501
    { id: 12, landfillId: 501, year: 2021, volumeM3: 60000, wasteTons: 200000, ch4Tons: 1550, co2eqTons: 39000 },
    { id: 13, landfillId: 501, year: 2022, volumeM3: 61000, wasteTons: 202000, ch4Tons: 1600, co2eqTons: 39500 },
    { id: 14, landfillId: 501, year: 2023, volumeM3: 62000, wasteTons: 205000, ch4Tons: 1650, co2eqTons: 40000 }
  ];

  // ===== Highcharts bindings =====
  Highcharts: typeof Highcharts = Highcharts;

  chartRegionWaste: Highcharts.Options = {};
  chartSpeciesPie: Highcharts.Options = {};
  chartEmissionsTrend: Highcharts.Options = {};
  chartPrediction: Highcharts.Options = {};

  // ===== Top lists / cards =====
  topLargestLandfills: { name: string; totalWaste: number; region: string }[] = [];
  mostPollutedRegion?: {
    region: string;
    ch4PerKm2: number;
    co2eqPerKm2: number;
    totalCh4: number;
    totalCo2eq: number;
    population: number;
    areaKm2: number;
  };

  // ===== Table =====
  displayedColumns: string[] = ['landfill', 'region', 'year', 'volumeM3', 'wasteTons', 'ch4Tons', 'co2eqTons'];
  dataSource = new MatTableDataSource<
    Monitoring & { landfillName: string; regionName: string }
  >([]);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  ngOnInit(): void {
    this.buildCharts();
    this.buildTopLists();
    this.buildTable();
  }

  // ----------------- Aggregations & Charts -----------------
  private sumBy<T>(arr: T[], sel: (x: T) => number) {
    return arr.reduce((acc, x) => acc + sel(x), 0);
  }

  private getRegionNameById(id: number) {
    return this.regions.find(r => r.id === id)?.name ?? '';
  }

  private getLandfillById(id: number) {
    return this.landfills.find(l => l.id === id);
  }

  private buildCharts() {
    // 1) Region waste (bar): total waste across all years by region
    const wasteByRegion = this.regions.map(r => {
      const regionLandfillIds = this.landfills.filter(l => l.regionId === r.id).map(l => l.id);
      const regionMon = this.monitoring.filter(m => regionLandfillIds.includes(m.landfillId));
      return {
        name: r.name,
        totalWaste: this.sumBy(regionMon, m => m.wasteTons)
      };
    });

    this.chartRegionWaste = {
      chart: { type: 'column' },
      title: { text: undefined },
      xAxis: { categories: wasteByRegion.map(x => x.name), title: { text: undefined } },
      yAxis: { title: { text: 't' } },
      tooltip: { pointFormat: '{series.name}: <b>{point.y:.0f} t</b>' },
      series: [
        { type: 'column', name: 'Waste (t)', data: wasteByRegion.map(x => x.totalWaste) }
      ]
    };

    // 2) Pie: landfill species distribution
    const speciesCounts = ['sanitary','unsanitary','wild'].map(kind => ({
      name: kind,
      y: this.landfills.filter(l => l.kind === kind).length
    }));
    this.chartSpeciesPie = {
      chart: { type: 'pie' },
      title: { text: undefined },
      series: [{ type: 'pie', name: 'Count', data: speciesCounts as any }]
    };

    // 3) Line: CH4 emissions over years (sum across all landfills)
    const years = Array.from(new Set(this.monitoring.map(m => m.year))).sort();
    const ch4ByYear = years.map(y =>
      this.sumBy(this.monitoring.filter(m => m.year === y), m => m.ch4Tons)
    );

    this.chartEmissionsTrend = {
      chart: { type: 'line' },
      title: { text: undefined },
      xAxis: { categories: years.map(y => y.toString()) },
      yAxis: { title: { text: 'CH₄ (t)' } },
      series: [{ type: 'line', name: 'CH₄', data: ch4ByYear }]
    };

    // 4) Prediction chart (simple linear projection of CH4 next 3 years)
    const projYears = [...years];
    const lastYear = years[years.length - 1];
    const nextYears = [lastYear + 1, lastYear + 2, lastYear + 3];
    // crude linear trend
    const trendPerYear =
      ch4ByYear.length >= 2
        ? (ch4ByYear[ch4ByYear.length - 1] - ch4ByYear[0]) / (ch4ByYear.length - 1)
        : 0;
    const projections = nextYears.map((y, i) => ch4ByYear[ch4ByYear.length - 1] + trendPerYear * (i + 1));

    this.chartPrediction = {
      chart: { type: 'line' },
      title: { text: undefined },
      xAxis: { categories: [...projYears, ...nextYears].map(String) },
      yAxis: { title: { text: 'CH₄ (t)' } },
      series: [
        { type: 'line', name: 'Observed', data: ch4ByYear },
        { type: 'line', name: 'Projected', data: [...new Array(ch4ByYear.length - 1).fill(null), ch4ByYear[ch4ByYear.length - 1], ...projections], dashStyle: 'Dash' }
      ]
    };
  }

  private buildTopLists() {
    // Top 3 largest landfills by total waste (all years)
    const wasteByLandfill = this.landfills.map(lf => {
      const mon = this.monitoring.filter(m => m.landfillId === lf.id);
      return {
        landfillId: lf.id,
        name: lf.name,
        totalWaste: this.sumBy(mon, m => m.wasteTons),
        region: this.getRegionNameById(lf.regionId)
      };
    }).sort((a, b) => b.totalWaste - a.totalWaste);

    this.topLargestLandfills = wasteByLandfill.slice(0, 3);

    // Most polluted region by CH4 per km² (needs area)
    const regionsWithArea = this.regions.filter(r => r.areaKm2 && r.areaKm2 > 0);
    const rank = regionsWithArea.map(r => {
      const ids = this.landfills.filter(l => l.regionId === r.id).map(l => l.id);
      const mon = this.monitoring.filter(m => ids.includes(m.landfillId));
      const totalCh4 = this.sumBy(mon, m => m.ch4Tons);
      const totalCo2eq = this.sumBy(mon, m => m.co2eqTons);
      const ch4PerKm2 = totalCh4 / (r.areaKm2 || 1);
      const co2eqPerKm2 = totalCo2eq / (r.areaKm2 || 1);
      return {
        region: r.name,
        totalCh4,
        totalCo2eq,
        ch4PerKm2,
        co2eqPerKm2,
        population: r.population,
        areaKm2: r.areaKm2!
      };
    }).sort((a, b) => b.ch4PerKm2 - a.ch4PerKm2);

    this.mostPollutedRegion = rank[0];
  }

  private buildTable() {
    const enriched = this.monitoring.map(m => {
      const lf = this.getLandfillById(m.landfillId)!;
      const regionName = this.getRegionNameById(lf.regionId);
      return {
        ...m,
        landfillName: lf.name,
        regionName
      };
    });

    this.dataSource = new MatTableDataSource(enriched);
  }

  ngAfterViewInit(): void {
    // Hook paginator/sort once view is ready
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  applyFilter(value: string) {
    this.dataSource.filter = value.trim().toLowerCase();
  }
}