import { isPlatformBrowser } from '@angular/common';
import { AfterViewInit, Component, ElementRef, OnDestroy, PLATFORM_ID, ViewChild, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-tubes-background',
  imports: [RouterLink],
  templateUrl: './tubes-background.html',
  styleUrl: './tubes-background.scss',
})
export class TubesBackgroundComponent implements AfterViewInit, OnDestroy {
  @ViewChild('canvas', { static: true }) canvas!: ElementRef<HTMLCanvasElement>;
  private readonly platformId = inject(PLATFORM_ID);
  private dispose?: () => void;

  async ngAfterViewInit(): Promise<void> {
    if (!isPlatformBrowser(this.platformId) || matchMedia('(prefers-reduced-motion: reduce)').matches) return;
    const THREE = await import('three');
    const canvas = this.canvas.nativeElement;
    const renderer = new THREE.WebGLRenderer({ canvas, antialias: true, alpha: true, powerPreference: 'high-performance' });
    renderer.setPixelRatio(Math.min(devicePixelRatio, 1.6));
    const scene = new THREE.Scene();
    const camera = new THREE.PerspectiveCamera(48, 1, 0.1, 100);
    camera.position.z = 10.5;
    const group = new THREE.Group();
    scene.add(group);
    const colors = [0x008ca5, 0x8ac1c8, 0x03484d, 0xdbeff1];
    for (let i = 0; i < 8; i++) {
      const points: InstanceType<typeof THREE.Vector3>[] = [];
      for (let j = 0; j < 9; j++) {
        const x = -6.2 + j * 1.55;
        const y = Math.sin(j * 0.9 + i * 0.72) * (0.55 + i * 0.025) + (i - 3.5) * 0.35;
        const z = Math.cos(j * 0.58 + i) * 0.62 + (i % 2 ? -0.4 : 0.3);
        points.push(new THREE.Vector3(x, y, z));
      }
      const curve = new THREE.CatmullRomCurve3(points);
      const geometry = new THREE.TubeGeometry(curve, 90, 0.045 + (i % 3) * 0.018, 8, false);
      const material = new THREE.MeshStandardMaterial({ color: colors[i % colors.length], roughness: 0.26, metalness: 0.26, transparent: true, opacity: 0.9 });
      group.add(new THREE.Mesh(geometry, material));
    }
    scene.add(new THREE.AmbientLight(0xffffff, 1.2));
    const key = new THREE.PointLight(0x8ac1c8, 22, 30); key.position.set(-2, 3, 6); scene.add(key);
    const accent = new THREE.PointLight(0x008ca5, 20, 30); accent.position.set(4, -3, 5); scene.add(accent);
    let targetX = 0; let targetY = 0; let raf = 0;
    const resize = () => {
      const rect = canvas.getBoundingClientRect();
      renderer.setSize(Math.max(1, rect.width), Math.max(1, rect.height), false);
      camera.aspect = rect.width / Math.max(1, rect.height); camera.updateProjectionMatrix();
    };
    const move = (event: PointerEvent) => {
      const rect = canvas.getBoundingClientRect();
      targetX = ((event.clientX - rect.left) / rect.width - 0.5) * 0.65;
      targetY = ((event.clientY - rect.top) / rect.height - 0.5) * 0.45;
    };
    const tick = (time: number) => {
      group.rotation.y += (targetX - group.rotation.y) * 0.035;
      group.rotation.x += (-targetY - group.rotation.x) * 0.035;
      group.position.y = Math.sin(time * 0.00045) * 0.14;
      renderer.render(scene, camera); raf = requestAnimationFrame(tick);
    };
    resize(); window.addEventListener('resize', resize); canvas.addEventListener('pointermove', move); raf = requestAnimationFrame(tick);
    this.dispose = () => {
      cancelAnimationFrame(raf); window.removeEventListener('resize', resize); canvas.removeEventListener('pointermove', move);
      group.traverse((obj) => { if (obj instanceof THREE.Mesh) { obj.geometry.dispose(); const material = obj.material; if (Array.isArray(material)) material.forEach((m) => m.dispose()); else material.dispose(); } });
      renderer.dispose();
    };
  }
  ngOnDestroy(): void { this.dispose?.(); }
}
