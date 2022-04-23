import {ResourcesService} from "../resources.service";
import {InjectModel} from "@nestjs/sequelize";
import {Music} from "./musics.model";
import {Resource} from "../resources.model";
import {NotFoundError} from "rxjs";

export class MusicsService {
    constructor(private readonly resourcesService: ResourcesService,
                @InjectModel(Music)private readonly musicRepository: typeof Music) {  }
    async createMusicAsync(name: string, ownerId: number, tags: string[], extension: string, uploadDate: Date, duration: number): Promise<Music> {
        if (!name) {
            throw new Error('Music name not provided');
        }
        if (!tags) {
            tags = []
        }
        if (!extension) {
            throw new Error('Music extension is not provided');
        }
        if (duration < 1) {
            throw new RangeError('Music duration must be positive');
        }
        let resource: Resource | null;
        let music: Music | null;
        try {
            resource = await this.resourcesService.createResourceAsync(name, ownerId, tags, extension, uploadDate);
            const music = await this.musicRepository.create({
                id: resource.id,
                duration: duration
            });
            if (!music) {
                throw new Error('Could not create music object');
            }
            return music;
        } catch (e) {
            if (resource) {
                await this.resourcesService.deleteResourceByIdAsync(resource.id);
            }
            if (music) {
                await music.destroy();
                await music.save();
            }
            throw e;
        }
    }

    async findMusicByIdAsync(id: number): Promise<Music> {
        const music = await this.musicRepository.findByPk(id, {
            include: [{all: true}]
        });
        if (!music) {
            throw new NotFoundError('Music with specified id not found');
        }
        return music;
    }

    async getAllMusicsPagedOrderedAsync(pageNumber: number, pageSize: number) {
        if (pageNumber < 1) throw new RangeError('Page number must be positive');
        if (pageSize < 1) throw new RangeError('Page size must be positive');
        return await this.musicRepository.findAndCountAll({
            include: [{all: true,}],
            limit: pageSize,
            offset: (pageNumber - 1) * pageSize,
            order: [['Id', 'ASC']]
        });
    }

    async deleteMusicByIdAsync(id: number) {
        const affected = await this.musicRepository.destroy({
            where: {
                id: id
            },
            cascade: true
        });
        if (affected === 0) {
            throw new NotFoundError('Music with specified id not found');
        }
    }
}