import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../pipes/translation-pipe';

// Highcharts
import * as Highcharts from 'highcharts';
import { HighchartsChartComponent } from 'highcharts-angular';

// Angular Material
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';

import { StatisticsService } from '../../services';
import { of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

// ---- DTOs your controller now returns ----
type WasteByRegion = { name: string; totalWaste: number };
type LandfillType = { name: string; count: number };
type Ch4OverTime = { years: number[]; ch4ByYear: number[] };
type TopLandfill = { name: string; region: string; areaM2?: number; yearCreated?: number };
type MostImpacted = { region: string; totalCh4: number };
type GrowthRow = { year: number; landfillCount: number };

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
    TranslatePipe,
  ],
  templateUrl: './statistics-component.html',
  styleUrls: ['./statistics-component.css'],
})
export class StatisticsComponent implements OnInit {
  Highcharts: typeof Highcharts = Highcharts;

  chartRegionWaste: Highcharts.Options = { accessibility: { enabled: false } };
  chartSpeciesPie: Highcharts.Options = { accessibility: { enabled: false } };
  chartEmissionsTrend: Highcharts.Options = { accessibility: { enabled: false } };
  chartPrediction: Highcharts.Options = { accessibility: { enabled: false } };

  topLargestLandfills: TopLandfill[] = [];
  mostPollutedRegion?: MostImpacted;

  // Table now uses the growth dataset shape returned by /landfill-growth
  displayedColumns: string[] = ['year', 'landfillCount'];
  dataSource = new MatTableDataSource<GrowthRow>([]);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(private statisticsService: StatisticsService) {}

  ngOnInit(): void {
    // Total waste by region
    this.statisticsService
      .getTotalWasteByRegion()
      .pipe(
        map((rows: WasteByRegion[] | any) => (Array.isArray(rows) ? rows : [])),
        catchError(() => of([] as WasteByRegion[]))
      )
      .subscribe((rows) => {
        const categories = rows.map((r) => r.name ?? 'N/A');
        const values = rows.map((r) => Number(r.totalWaste ?? 0));
        this.chartRegionWaste = {
          accessibility: { enabled: false },
          chart: { type: 'column' },
          title: { text: undefined },
          xAxis: { categories, title: { text: undefined } },
          yAxis: { title: { text: 't' } },
          tooltip: { pointFormat: '{series.name}: <b>{point.y:.0f} t</b>' },
          series: [{ type: 'column', name: 'Waste (t)', data: values }],
        };
      });

    // Landfill types
    this.statisticsService
      .getLandfillTypes()
      .pipe(
        map((rows: LandfillType[] | any) => (Array.isArray(rows) ? rows : [])),
        catchError(() => of([] as LandfillType[]))
      )
      .subscribe((rows) => {
        this.chartSpeciesPie = {
          accessibility: { enabled: false },
          chart: { type: 'pie' },
          title: { text: undefined },
          series: [
            {
              type: 'pie',
              name: 'Count',
              data: rows.map((r) => ({ name: r.name, y: Number(r.count ?? 0) })),
            },
          ],
        };
      });

    // CH4 emissions over time
    this.statisticsService
      .getCh4EmissionsOverTime()
      .pipe(
        map((res: Ch4OverTime | any) =>
          res && Array.isArray(res.years) && Array.isArray(res.ch4ByYear)
            ? res
            : ({ years: [], ch4ByYear: [] } as Ch4OverTime)
        ),
        catchError(() => of({ years: [], ch4ByYear: [] } as Ch4OverTime))
      )
      .subscribe((data) => {
        this.chartEmissionsTrend = {
          accessibility: { enabled: false },
          chart: { type: 'line' },
          title: { text: undefined },
          xAxis: { categories: data.years.map(String) },
          yAxis: { title: { text: 'CH₄ (t)' } },
          series: [{ type: 'line', name: 'CH₄', data: data.ch4ByYear }],
        };
      });

    // Top 3 largest landfills
    this.statisticsService
      .getTop3LargestLandfills()
      .pipe(
        map((rows: TopLandfill[] | any) => (Array.isArray(rows) ? rows : [])),
        catchError(() => of([] as TopLandfill[]))
      )
      .subscribe((rows) => (this.topLargestLandfills = rows));

    // Most impacted region
    this.statisticsService
      .getMostImpactedRegion()
      .pipe(catchError(() => of(undefined)))
      .subscribe((data) => (this.mostPollutedRegion = data as MostImpacted | undefined));

    // Table: landfill growth over years
    this.statisticsService
      .getLandfillGrowthOverYears()
      .pipe(
        map((rows: GrowthRow[] | any) => (Array.isArray(rows) ? rows : [])),
        catchError(() => of([] as GrowthRow[]))
      )
      .subscribe((rows) => {
        this.dataSource = new MatTableDataSource(rows);
        if (this.paginator) this.dataSource.paginator = this.paginator;
        if (this.sort) this.dataSource.sort = this.sort;
      });
  }

  ngAfterViewInit(): void {
    if (this.paginator) this.dataSource.paginator = this.paginator;
    if (this.sort) this.dataSource.sort = this.sort;
  }

  applyFilter(value: string) {
    this.dataSource.filter = value.trim().toLowerCase();
  }
}
